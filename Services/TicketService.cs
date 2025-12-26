using Microsoft.EntityFrameworkCore;
using TicketSales.Data;
using TicketSales.Models;
using TicketSales.Models.ViewModels;
using QRCoder;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TicketSales.Services
{
    public class TicketService
    {
        private readonly TicketContext _ticketContext;

        public TicketService(TicketContext context)
        {
            _ticketContext = context;
        }

        // --- MÉTODOS DE COMPRA ---

        public List<Compra> ListarTodasCompras()
        {
            return _ticketContext.Compras
                .Include(c => c.Cliente)
                .Include(c => c.Evento)
                .Include(c => c.AssentosSelecionados).ToList();
        }


        public List<Compra> ListarComprasPorEvento(int eventoId)
        {
            return _ticketContext.Compras
                .Include(c => c.Cliente) // boa pratica incluir cliente
                .Where(c => c.Evento.Id == eventoId).ToList();
        }


        // Métodos de Compra: POST

        public void RegistrarCompra(int clinteId, int eventoId, List<int> assentosIds, TiposDePagamento metodoPagamento)
        {
            var cliente = _ticketContext.Clientes.Find(clinteId);

            var evento = _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == eventoId);


            if (cliente == null || !cliente.Ativo) throw new Exception("Cliente inválido ou inativo");

            if (evento == null || !evento.Ativo) throw new Exception("Evento inválido ou inativo");

            if (assentosIds == null || !assentosIds.Any()) throw new Exception("Nenhum assento foi selecionado.");



            // Validar Assentos
            var assentosSelecionados = evento.Assentos
                .Where(a => assentosIds.Contains(a.Id) && !a.Ocupado)
                .ToList();

            if (assentosSelecionados.Count != assentosIds.Count)
                throw new Exception("Um ou mais assentos selecionados não estão disponíveis.");

            foreach (var assento in assentosSelecionados)
            {
                assento.Ocupado = true;
                _ticketContext.Entry(assento).State = EntityState.Modified;
            }
            evento.LugaresDisponiveis -= assentosSelecionados.Count;
            _ticketContext.Entry(evento).State = EntityState.Modified; // Força update do evento também


            var compra = new Compra
            {
                Cliente = cliente,
                Evento = evento,
                AssentosSelecionados = assentosSelecionados,
                ValorTotal = evento.Valor * assentosSelecionados.Count,
                MetodoPagamento = metodoPagamento,
                DataCompra = DateTime.Now
            };

            _ticketContext.Compras.Add(compra);
            _ticketContext.SaveChanges();
        }

        public void CancelarCompra(int compraId)
        {
            var compra = _ticketContext.Compras
                .Include(c => c.AssentosSelecionados)
                .Include(c => c.Evento)
                .FirstOrDefault(c => c.Id == compraId);

            if (compra == null)
                throw new Exception("Compra não encontrada.");

            // Liberar assentos
            foreach (var assento in compra.AssentosSelecionados)
            {
                assento.Ocupado = false;
            }

            if (compra.Evento != null)
            {
                compra.Evento.LugaresDisponiveis += compra.AssentosSelecionados.Count;
            }

            compra.Evento.LugaresDisponiveis += compra.AssentosSelecionados.Count;

            _ticketContext.Compras.Remove(compra);
            _ticketContext.SaveChanges();
        }


        // --- MÉTODOS DE EVENTO ---

        public List<Evento> ListarEventosAtivos()
        {
            return _ticketContext.Eventos
                .Where(e => e.Ativo).ToList();
        }

        public Evento ObterEventoPorId(int id)
        {
            return _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == id);
        }

        public List<Assento> ListarAssentosDoEvento(int eventoId)
        {
            return _ticketContext.Assentos
                .Where(a => a.EventoId == eventoId)
                .ToList();

        }

        public Evento ObterEventoParaEdicao(int id, string userId, bool isAdmin)
        {
            var evento = _ticketContext.Eventos.Find(id);

            if (evento == null) throw new Exception("evento não encontrado");

            if (!isAdmin && evento.OrganizadorId != userId)
            {
                throw new Exception("Você não tem permissão para editar esse evento. ");
            }

            return evento;
        }

        // Metodos Evento de ação: POST

        public void CriarEvento(string nome, int quantidadeLugares, decimal valor, string categoria, string? nomeImagem, string organizadorId, string local, string endereco, DateTime dataEvento, double? lat, double? lon)
        {

            if (dataEvento <= DateTime.Now)
            {
                throw new Exception("A data do evento deve ser futura.");
            }

            if (string.IsNullOrWhiteSpace(nome))
                throw new Exception("Digite um nome para o evento.");

            if (quantidadeLugares <= 0)
                throw new Exception("Coloque uma quantidade válida de lugares.");

            if (valor < 0)
                throw new Exception("Digite um valor válido.");

            var listaAssentos = new List<Assento>();
            for (int i = 1; i <= quantidadeLugares; i++)
            {
                listaAssentos.Add(new Assento
                {
                    CodigoAssento = $"A{i}",
                    Ocupado = false
                });
            }

            var evento = new Evento
            {
                Nome = nome,
                QuantidadeLugares = quantidadeLugares,
                LugaresDisponiveis = quantidadeLugares, // inicia com todos disponíveis
                Valor = valor,
                Categoria = string.IsNullOrEmpty(categoria) ? "Geral" : categoria,
                Ativo = true,
                DataCriacao = DateTime.Now,
                DataEvento = dataEvento,
                Assentos = listaAssentos,
                Imagem = nomeImagem,
                OrganizadorId = organizadorId,
                Local = local,
                Endereco = endereco,
                Latitude = lat,
                Longitude = lon
            };

            _ticketContext.Eventos.Add(evento);
            _ticketContext.SaveChanges();
        }


        public void DesativarEvento(int id, string userId, bool isAdmin)
        {
            var evento = _ticketContext.Eventos.Find(id);

            if (evento == null) throw new Exception("Evento não encontrado");

            if (!isAdmin && evento.OrganizadorId != userId)
            {
                throw new Exception("Você não tem permissão para alterar este evento");
            }

            if (!evento.Ativo) throw new Exception("Evento já está desativado");



            evento.Ativo = false;
            _ticketContext.SaveChanges();

        }

        public void SelecionarAssentos(int eventoId, List<string> codigosAssentos)
        {
            var evento = _ticketContext.Eventos
                .Include(e => e.Assentos)
                .FirstOrDefault(e => e.Id == eventoId && e.Ativo);

            if (evento == null) throw new Exception("Evento não encontrado ou inativo.");


            // Filtro dos assentos disponíveis
            var assentosDisponiveis = evento.Assentos
            .Where(a => codigosAssentos.Contains(a.CodigoAssento) && !a.Ocupado)
            .ToList();

            if (assentosDisponiveis.Count != codigosAssentos.Count)
                throw new Exception("Um ou mais assentos selecionados não estão disponíveis.");

            if (assentosDisponiveis.Count > evento.LugaresDisponiveis)
                throw new Exception("Número de assentos selecionados excede os lugares disponíveis.");

            // atualiza o status dos assentos para ocupado
            foreach (var assento in assentosDisponiveis)
            {
                assento.Ocupado = true;
            }

            evento.LugaresDisponiveis -= assentosDisponiveis.Count;
            _ticketContext.SaveChanges();

        }

        public List<Evento> ListarEventosParaGerenciamento(string userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return _ticketContext.Eventos.ToList();
            }

            return _ticketContext.Eventos
                .Where(e => e.OrganizadorId == userId)
                .ToList();
        }

        public void AtualizarEvento(int id, Evento dadosNovos, string? novaImagem, string userId, bool isAdmin)
        {
            var eventoNoBanco = _ticketContext.Eventos.Find(id);

            if (eventoNoBanco == null) throw new Exception("Evento não encontrado. ");

            if (!isAdmin && eventoNoBanco.OrganizadorId != userId)
            {
                throw new Exception("Sem permissão");
            }

            eventoNoBanco.Nome = dadosNovos.Nome;
            eventoNoBanco.Valor = dadosNovos.Valor;
            eventoNoBanco.Categoria = dadosNovos.Categoria;
            eventoNoBanco.Local = dadosNovos.Local;
            eventoNoBanco.Endereco = dadosNovos.Endereco;
            eventoNoBanco.Latitude = dadosNovos.Latitude;
            eventoNoBanco.Longitude = dadosNovos.Longitude;
            eventoNoBanco.DataEvento = dadosNovos.DataEvento;

            if (!string.IsNullOrEmpty(novaImagem))
            {
                eventoNoBanco.Imagem = novaImagem;
            }

            _ticketContext.Eventos.Update(eventoNoBanco);
            _ticketContext.SaveChanges();
        }

        public void AtivarEvento (int id, bool isAdmin, string userId)
        {
            var evento = _ticketContext.Eventos.Find(id);

            if (evento == null) throw new Exception("Evento não encontrado.");
            
            if (!isAdmin)
            {
                throw new Exception("Apenas administradores podem reativar eventos. Entre em contato com o suporte.");
            }
            if (evento != null)
            {
                evento.Ativo = true;

                _ticketContext.SaveChanges();
            }
            
        }

        // --- MÉTODOS DE USUARIO ---

        public List<Cliente> ListarClientesAtivos()
        {
            return _ticketContext.Clientes.Where(c => c.Ativo).ToList();
        }

        public void CadastrarCliente(string nome, string email, int idade)
        {
            if (string.IsNullOrEmpty(nome)) throw new ArgumentException("Digite um nome válido");

            if (idade < 18) throw new ArgumentException("É necessário ser maior de 18 anos");

            if (string.IsNullOrEmpty(email) || !email.Contains('@')) throw new ArgumentException("Digite um E-mail válido");

            var clientes = new Cliente
            {
                Nome = nome,
                Email = email,
                Idade = idade,
                Ativo = true,
                DataCadastro = DateTime.Now,
            };

            _ticketContext.Clientes.Add(clientes);
            _ticketContext.SaveChanges();
        }

        public string DesativarCliente(int id)
        {
            var cliente = _ticketContext.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null) throw new Exception("Cliente não encontrado");
            if (!cliente.Ativo) throw new Exception("O cliente não está ativo");

            cliente.Ativo = false;

            _ticketContext.SaveChanges();
            return $"O cliente: {cliente.Nome} foi desativado com sucesso";
        }

        public void CadastrarClienteVinculado(string nome, string email, int idade, string usuarioId)
        {
            if (string.IsNullOrEmpty(nome)) throw new ArgumentException("Digite um nome válido");

            if (idade < 18) throw new ArgumentException("É necessário ser maior de 18 anos");

            if (string.IsNullOrEmpty(email) || !email.Contains('@')) throw new ArgumentException("Digite um E-mail válido");

            var clientes = new Cliente
            {
                Nome = nome,
                Email = email,
                Idade = idade,
                Ativo = true,
                DataCadastro = DateTime.Now,
                UsuarioId = usuarioId
            };

            _ticketContext.Clientes.Add(clientes);
            _ticketContext.SaveChanges();
        }

        public Cliente ObterClientePorUsuarioId(string usuarioId)
        {
            return _ticketContext.Clientes
                .FirstOrDefault(c => c.UsuarioId == usuarioId);
        }

        public DashboardViewModel ObterDadosDashboard(string userId, bool isAdmin)
        {
            var model = new DashboardViewModel();

            var queryEventos = _ticketContext.Eventos.AsQueryable();

            if (!isAdmin)
            {
                queryEventos = queryEventos.Where(e => e.OrganizadorId == userId);
            }

            var eventosIds = queryEventos.Select(e => e.Id).ToList();

            var queryCompras = _ticketContext.Compras
                .Include(c => c.Evento)
                .AsQueryable();

            if (!isAdmin)
            {
                queryCompras = queryCompras.Where(c => eventosIds.Contains(c.EventoId));
            }

            var queryAssentos = _ticketContext.Assentos.AsQueryable();

            if (isAdmin)
            {
                queryAssentos = queryAssentos.Where(a => eventosIds.Contains(a.EventoId));
            }

            model.FaturamentoTotal = queryCompras.Sum(c => c.ValorTotal);

            model.TotalIngressosVendidos = queryAssentos.Count(a => a.Ocupado);

            model.TotalEventosAtivos = queryEventos.Count(e => e.Ativo);

            if (isAdmin)
            {
                model.TotalClientes = _ticketContext.Clientes.Count();

            }

            else
            {
                model.TotalClientes = queryCompras.Select(c => c.ClienteId).Distinct().Count();
            }

            var dadosGraficos = queryCompras
            .GroupBy(c => c.Evento.Nome)
            .Select(grupo => new
        {
            NomeEvento = grupo.Key,
            TotalVendido = grupo.Sum(c => c.ValorTotal)
        })
            .OrderByDescending(x => x.TotalVendido)
            .Take(5)
            .ToList();

            model.LabelsGrafico = dadosGraficos.Select(x => x.NomeEvento).ToList();
            model.DadosGrafico = dadosGraficos.Select(x => x.TotalVendido).ToList();

            return model;
        }



        // --- MÉTODO QR CODE ---

        public byte[] GerarBytesQRCode(int compraId, string userId)
        {

            var compra = _ticketContext.Compras
                .Include(c => c.Evento)
                .Include(c => c.AssentosSelecionados)
                .Include(c => c.Cliente)
                .FirstOrDefault(c => c.Id == compraId && c.Cliente.UsuarioId == userId);

            if (compra == null) return null;

            // Motangem do conteúdo do QR Code

            var textoNota = $"TICKET SALES - COMPROVANTE\n" +
                            $"PEDIDO: {compra.Id}\n" +
                            $"EVENTO: {compra.Evento.Nome}\n" +
                            $"DATA: {compra.Evento.DataEvento:dd/MM/yyyy HH:mm}\n" +
                            $"ASSENTOS: {string.Join(", ", compra.AssentosSelecionados.Select(a => a.CodigoAssento))}\n" +
                            $"VALOR: {compra.ValorTotal:N2} \n" +
                            $"CLIENTE: {compra.Cliente.Nome}\n";

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(textoNota, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    // retorna array de bytes da imagem
                    return qrCode.GetGraphic(20);
                }
            }


        }
        public TicketPdfViewModel DownloadTicket(int id, string userId)
        {
            var compra = _ticketContext.Compras
                .Include(c => c.Evento)
                .Include(c => c.AssentosSelecionados)
                .Include(c => c.Cliente)
                .FirstOrDefault(c => c.Id == id && c.Cliente.UsuarioId == userId);

            if (compra == null) return null;

            var qrBytes = GerarBytesQRCode(id, userId);

            string qrCodeImgSrc = "";

            // Converte os bytes para Base64
            // Isso é necessário para o PDF conseguir renderizar a imagem sem precisar salvar arquivo no disco
            if (qrBytes != null)
            {
                var base64Qr = Convert.ToBase64String(qrBytes);
                qrCodeImgSrc = string.Format("data:image/png;base64,{0}", base64Qr);
            }

            return new TicketPdfViewModel
            {
                Compra = compra,
                QrCodeBase64 = qrCodeImgSrc
            };

            
        }

        // AJAX

        public List<Evento> FiltrarEventos(string termo, string categoria)
        {
            var query = _ticketContext.Eventos
                .Where(e => e.Ativo && e.DataEvento > DateTime.Now)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(termo))
            {
                termo = termo.Trim();
                query = query.Where(e => e.Nome.Contains(termo));
            }
            
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(e => e.Categoria == categoria);
            }
            return query.OrderBy(e => e.DataEvento).ToList();
        }
    }
    
}
