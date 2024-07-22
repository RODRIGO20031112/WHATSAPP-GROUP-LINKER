using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        var url = "https://gruposwhats.app/";
        var httpClient = new HttpClient();
        var htmlDoc = new HtmlDocument();

        var html = await httpClient.GetStringAsync(url);
        htmlDoc.LoadHtml(html);

        var divNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section[1]/div/div[1]");

        if (divNode != null)
        {
            var anchorNodes = divNode.SelectNodes(".//a");

            if (anchorNodes != null)
            {
                string[] options = new string[anchorNodes.Count];
                Console.WriteLine("Escolha uma das opções a seguir:");

                int counter = 1;
                for (int i = 0; i < anchorNodes.Count; i++)
                {
                    var anchor = anchorNodes[i];
                    var href = anchor.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(href))
                    {
                        var parts = href.Split('/');
                        var lastSegment = parts[^1];
                        options[i] = lastSegment;
                        Console.WriteLine($"{counter}. {lastSegment}");
                        counter++;
                    }
                }

                Console.Write("Digite o número da opção desejada: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int choice) && choice > 0 && choice <= options.Length)
                {
                    Console.WriteLine($"Você escolheu a opção: {options[choice - 1]}");

                    var status = true;
                    var statusCounter = 1;
                    int globalCounter = 1;

                    while (status)
                    {
                        try
                        {
                            var urlEscolhida = $"https://gruposwhats.app/category/{options[choice - 1]}?page={statusCounter}";
                            var htmlFiltrado = await httpClient.GetStringAsync(urlEscolhida);
                            htmlDoc.LoadHtml(htmlFiltrado);

                            var divNodeFiltrado = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section[2]/div/div");

                            if (divNodeFiltrado != null)
                            {
                                var anchorNodesFiltrados = divNodeFiltrado.SelectNodes(".//a");

                                if (anchorNodesFiltrados != null)
                                {
                                    //Console.WriteLine($"Página {statusCounter}:");
                                    int counterII = globalCounter;

                                    foreach (var anchor in anchorNodesFiltrados)
                                    {
                                        var href = anchor.GetAttributeValue("href", string.Empty);
                                        if (!string.IsNullOrEmpty(href))
                                        {
                                            var parts = href.Split('/');
                                            var lastSegment = parts[^1];
                                            await PrintWhatsAppGroupUrlAsync(httpClient, lastSegment);
                                            counterII++;
                                        }
                                    }

                                    globalCounter = counterII;

                                    var nextPageNode = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(text(),'Próxima') or contains(text(),'Next')]");
                                    if (nextPageNode == null)
                                    {
                                        status = false;
                                    }
                                    else
                                    {
                                        statusCounter++;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Nenhuma tag <a> encontrada na página filtrada.");
                                    status = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Div especificada não encontrada na página filtrada.");
                                status = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro: {ex.Message}");
                            status = false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Opção inválida.");
                }
            }
            else
            {
                Console.WriteLine("Nenhuma tag <a> encontrada dentro da div especificada.");
            }
        }
        else
        {
            Console.WriteLine("Div especificada não encontrada.");
        }
    }

    static async Task PrintWhatsAppGroupUrlAsync(HttpClient httpClient, string lastSegment)
    {
        var urlFiltradaWhatsapp = $"https://gruposwhats.app/group/{lastSegment}";
        string htmlWhatsapp = null;

        for (int i = 0; i < 5; i++)
        {
            try
            {
                htmlWhatsapp = await httpClient.GetStringAsync(urlFiltradaWhatsapp);
                if (!string.IsNullOrEmpty(htmlWhatsapp))
                    break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar a página: {ex.Message}");
            }

            await Task.Delay(0); 
        }

        if (string.IsNullOrEmpty(htmlWhatsapp))
        {
            Console.WriteLine("Não foi possível obter o conteúdo da página.");
            return;
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlWhatsapp);

        var TagAFiltradoWhatsapp = htmlDoc.DocumentNode.SelectSingleNode("/html/body/section[2]/div/div/div[2]/div/div/a");
        var urlWhatsapp = TagAFiltradoWhatsapp?.GetAttributeValue("data-url", string.Empty);
        Console.WriteLine(urlWhatsapp);
    }
}
