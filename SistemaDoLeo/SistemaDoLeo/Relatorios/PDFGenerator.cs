using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SistemaDoLeo.Modelos.Classes;
using System.Threading.Tasks;

namespace SistemaDoLeo.Relatorios
{
    public class PDFGenerator
    {
        // itextShar Report
        public async Task<MemoryStream> RelatorioDetalhesPedido(PedidoDetalhado pedido, List<PedidoItemDetalhado> listaItens)
        {
            var memoryStream = new MemoryStream();
            float margeLeft = 1.5f;
            float margeRight = 1.5f;
            float margeTop = 2.0f;
            float margeBottom = 1.0f;

            Document pdf = new Document(
                PageSize.A4,
                margeLeft.ToDpi(),
                margeRight.ToDpi(),
                margeTop.ToDpi(),
                margeBottom.ToDpi()
                );

            pdf.AddTitle("Sistema do Leo - Pedido");
            pdf.AddAuthor("Leonardo da Silva Fanck");
            pdf.AddCreationDate();
            pdf.AddKeywords("SistemaDoLeo");
            pdf.AddSubject("Blazor PDF");

            PdfWriter writer = PdfWriter.GetInstance(pdf, memoryStream);

            // CRIAR CABEÇALHO
            var fontStyle = FontFactory.GetFont("Arial", 26, BaseColor.White);
            var labelHeader = new Chunk($"Pedido de {pedido.TipoOperacao}", fontStyle);

            HeaderFooter header = new HeaderFooter(new Phrase(labelHeader), false)
            {
                BackgroundColor = new BaseColor(50, 20, 120),
                Alignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER
            };

            pdf.Header = header;

            // CRIANDO RODA PE
            fontStyle = FontFactory.GetFont("Arial", 13, BaseColor.White);
            var labelFooter = new Chunk($"Página: ", fontStyle);

            HeaderFooter footer = new HeaderFooter(new Phrase(labelFooter), true)
            {
                BackgroundColor = new BaseColor(120, 3, 120),
                Alignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER
            };
            pdf.Footer = footer;

            // CORPO
            pdf.Open();

            // Criar uma tabela com duas colunas
            PdfPTable infoTable = new PdfPTable(4);
            infoTable.WidthPercentage = 100;
            infoTable.SpacingBefore = 10f;
            infoTable.SpacingAfter = 20f;
            //infoTable.DefaultCell.Border = PdfCell.NO_BORDER;

            float tamanho = 13f;

            // Adicionar campos desejados (em duas colunas)
            infoTable.AddCell(new PdfPCell(new Phrase("Pedido:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f });
            infoTable.AddCell(new PdfPCell(new Phrase($"{pedido.Id}", new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -30f });
            infoTable.AddCell(new PdfPCell(new Phrase("Valor:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f });
            infoTable.AddCell(new PdfPCell(new Phrase($"R$ {pedido.Valor}", new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -20f });
            if (pedido.TipoOperacao.Equals("Venda"))
            {
                infoTable.AddCell(new PdfPCell(new Phrase("Cliente:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f, PaddingTop = 7f });
                infoTable.AddCell(new PdfPCell(new Phrase(pedido.ClienteNome, new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -30f, PaddingTop = 8f });
            }
            else
            {
                infoTable.AddCell(new PdfPCell(new Phrase("Fornecedor:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f, PaddingTop = 7f });
                infoTable.AddCell(new PdfPCell(new Phrase(pedido.ClienteNome, new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -10f, PaddingTop = 8f });
            }
            infoTable.AddCell(new PdfPCell(new Phrase("Desconto:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f, PaddingTop = 7f });
            infoTable.AddCell(new PdfPCell(new Phrase($"{pedido.Desconto}%", new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -21f, PaddingTop = 7f });
            infoTable.AddCell(new PdfPCell(new Phrase("Forma de Pagamento:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f, PaddingTop = 7f });
            infoTable.AddCell(new PdfPCell(new Phrase(pedido.FormaPgtoNome, new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -10f, PaddingTop = 9f });
            infoTable.AddCell(new PdfPCell(new Phrase("Total:", new Font(Font.HELVETICA, tamanho, Font.BOLD))) { Border = PdfPCell.NO_BORDER, PaddingLeft = 40f, PaddingTop = 7f });
            infoTable.AddCell(new PdfPCell(new Phrase($"R$ {pedido.Total}", new Font(Font.HELVETICA, tamanho))) { Border = PdfPCell.NO_BORDER, PaddingLeft = -20f, PaddingTop = 7f });

            // Adicionar a tabela ao documento
            pdf.Add(infoTable);

            // ITENS
            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100;
            float[] tamanhoColunas = { 1f, 5f, 3f, 2.3f, 2.3f, 3f };
            table.SetWidths(tamanhoColunas);

            table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;

            table.AddCell(new PdfPCell(new Phrase("Cod", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Produto", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Valor", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Quantidade", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Desconto", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Total", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Adicionar linhas de dados (exemplo, ajuste conforme necessário)
            foreach (var item in listaItens)
            {
                table.AddCell($"{item.ProdutoId}");
                table.AddCell(new PdfPCell(new Phrase($"{item.ProdutoNome}")) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell($"R$ {item.Valor}");
                table.AddCell($"{item.Quantidade}");
                table.AddCell($"{item.Desconto}%");
                table.AddCell($"R$ {item.Total}");
            }
            pdf.Add(table);

            pdf.Close();

            return memoryStream;
        }


        public async Task<MemoryStream> RelatorioPedidos(List<PedidoDetalhado> listaPedidos, string tipoOperacao)
        {
            var memoryStream = new MemoryStream();
            float margeLeft = 1.5f;
            float margeRight = 1.5f;
            float margeTop = 2.0f;
            float margeBottom = 1.0f;

            Document pdf = new Document(
                PageSize.A4,
                margeLeft.ToDpi(),
                margeRight.ToDpi(),
                margeTop.ToDpi(),
                margeBottom.ToDpi()
                );

            pdf.AddTitle("Sistema do Leo - Relatório de Pedidos");
            pdf.AddAuthor("Leonardo da Silva Fanck");
            pdf.AddCreationDate();
            pdf.AddKeywords("SistemaDoLeo");
            pdf.AddSubject("Blazor PDF");

            PdfWriter writer = PdfWriter.GetInstance(pdf, memoryStream);

            // CRIAR CABEÇALHO
            var fontStyle = FontFactory.GetFont("Arial", 26, BaseColor.White);
            var labelHeader = new Chunk($"Relatório de Pedidos de {tipoOperacao}", fontStyle);

            HeaderFooter header = new HeaderFooter(new Phrase(labelHeader), false)
            {
                BackgroundColor = new BaseColor(50, 20, 120),
                Alignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER
            };

            pdf.Header = header;

            // CRIANDO RODA PE
            fontStyle = FontFactory.GetFont("Arial", 13, BaseColor.White);
            var labelFooter = new Chunk($"Página: ", fontStyle);

            HeaderFooter footer = new HeaderFooter(new Phrase(labelFooter), true)
            {
                BackgroundColor = new BaseColor(120, 3, 120),
                Alignment = Element.ALIGN_CENTER,
                Border = Rectangle.NO_BORDER
            };
            pdf.Footer = footer;

            // CORPO
            pdf.Open();

            // GRID
            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100;
            float[] tamanhoColunas = { 1.55f, 5f, 3f, 2.3f, 3f, 2.3f };
            table.SetWidths(tamanhoColunas);

            table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;

            table.AddCell(new PdfPCell(new Phrase("Pedido", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            if (tipoOperacao.Equals("Venda"))
            {
                table.AddCell(new PdfPCell(new Phrase("Cliente", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }
            else
            {
                table.AddCell(new PdfPCell(new Phrase("Fornecedor", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            }
            table.AddCell(new PdfPCell(new Phrase("Valor", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Desconto", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Total", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Data", new Font(Font.HELVETICA, 12f, Font.BOLD))) { HorizontalAlignment = Element.ALIGN_CENTER });

            // Adicionar linhas de dados (exemplo, ajuste conforme necessário)
            foreach (var pedido in listaPedidos)
            {
                table.AddCell($"{pedido.Id}");
                table.AddCell(new PdfPCell(new Phrase($"{pedido.ClienteNome}")) { HorizontalAlignment = Element.ALIGN_LEFT });
                table.AddCell($"R$ {pedido.Valor}");
                table.AddCell($"{pedido.Desconto}%");
                table.AddCell($"R$ {pedido.Total}");
                table.AddCell($"{pedido.Data.ToString("dd/MM/yyyy")}");
            }
            pdf.Add(table);

            pdf.Close();

            return memoryStream;
        }

    }
}
