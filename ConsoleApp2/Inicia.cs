using ConsoleApp2.Api;
using ConsoleApp2.Helper;
using Microsoft.Playwright;

namespace ConsoleApp2
{
    internal static class Inicia
    {
        public static IPage Page { get; set; }
        public static async Task<string> Start()
        {
            try
            {

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(
                   new BrowserTypeLaunchOptions { Channel = "chrome", Headless = false, SlowMo = 40, Timeout = 0, DownloadsPath = "", Args = new List<string>() { "--start-maximized" } });





                Page = await browser.NewPageAsync(
                    new BrowserNewPageOptions
                    {
                        ViewportSize = ViewportSize.NoViewport,
                        AcceptDownloads = true,
                        Locale = "pt-BR",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246"
                    }
                );



                var urlAtual = Page.Url;

                await Page.GotoAsync("https://cvmweb.cvm.gov.br/SWB/default.asp?sg_sistema=scw");

                await Page.RouteAsync("**/*", route => route.FulfillAsync(new RouteFulfillOptions { Body = "<html></html>" }));
                


                await Page.UnrouteAsync("**/*");

                List<Cookie> cookies = new List<Cookie>();

                cookies.Add(new Cookie { /*Domain = ".sso.acesso.gov.br", */Url = urlAtual, Name = "INGRESSCOOKIE", Value = "72aa6d20e34cd2d6" });
                cookies.Add(new Cookie { /*Domain = ".sso.acesso.gov.br", */Url = urlAtual, Name = "Govbrcoord", Value = "CLTIzLjU4ODRfLTQ2LjkwMjc%3D" });
                cookies.Add(new Cookie { /*Domain = ".sso.acesso.gov.br", */Url = urlAtual, Name = "Session_Gov_Br_Prod", Value = "q3idyFTX84wfE5G8ZmgZiLPCpRGIrvPoKQMGoJrn.scp-84579fd64c-9hz92" });
                cookies.Add(new Cookie { /*Domain = ".sso.acesso.gov.br", */Url = urlAtual, Name = "Govbrid", Value = "9dba799f-e8de-437a-a3d2-df319959cf9a" });

                await Page.FrameLocator("frame[name=\"Main\"]").FrameLocator("frame[name=\"SubMain\"]").GetByRole(AriaRole.Link, new() { Name = "Acesso Gov BR" }).ClickAsync();

                Thread.Sleep(25000);

                try
                {
                    var context = browser.Contexts.FirstOrDefault();
                    await context.AddCookiesAsync(cookies);

                }
                catch (Exception ex)
                {

                    throw;
                }


                await Page.GetByPlaceholder("Digite seu CPF").ClickAsync();
                Thread.Sleep(1320);
                await Page.GetByPlaceholder("Digite seu CPF").TypeAsync("157.010.968-07");
                Thread.Sleep(2030);
                await Page.GetByRole(AriaRole.Button, new() { Name = "Continuar" }).ClickAsync();
                Thread.Sleep(1340);
                await Page.GetByPlaceholder("Digite sua senha atual").ClickAsync();
                Thread.Sleep(1120);
                await Page.GetByPlaceholder("Digite sua senha atual").TypeAsync("Th0m45@K03n");
                Thread.Sleep(2040);

                await Page.GetByLabel("Botão Entrar. Aperte a tecla enter para entrar.").ClickAsync();

                try
                {
                    QuebrarCaptcha(urlAtual);
                    Thread.Sleep(5000);

                    await Page.PauseAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return "";

        }

        private static void QuebrarCaptcha(string urlAtual)
        {
            DebugHelper.VerboseMode = true;

            var api = new HCaptchaProxyless
            {
                ClientKey = "7e63e104e9bdedfe4e53c0438d3e3e66",
                WebsiteUrl = new Uri($"{urlAtual}"),
                WebsiteKey = "93b08d40-d46c-400a-ba07-6f91cda815b9",
                SoftId = 0

            };



            if (!api.CreateTask())
                DebugHelper.Out("API v2 send failed. " + api.ErrorMessage, DebugHelper.Type.Error);
            else if (!api.WaitForResult())
                DebugHelper.Out("Could not solve the captcha.", DebugHelper.Type.Error);
            else
                DebugHelper.Out("Result: " + api.GetTaskSolution().GRecaptchaResponse, DebugHelper.Type.Success);

        }
    }
}
