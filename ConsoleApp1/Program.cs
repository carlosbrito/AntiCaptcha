using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
        }

        public async Task<bool> Get()
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

            await Page.GotoAsync("https://cvmweb.cvm.gov.br/SWB/default.asp?sg_sistema=scw");

            await Page.FrameLocator("frame[name=\"Main\"]").FrameLocator("frame[name=\"SubMain\"]").GetByRole(AriaRole.Link, new() { Name = "Acesso Gov BR" }).ClickAsync();
            await Page.GetByPlaceholder("Digite seu CPF").ClickAsync();
            await Page.GetByPlaceholder("Digite seu CPF").TypeAsync("157.010.968-07");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Continuar" }).ClickAsync();
            await Page.GetByPlaceholder("Digite sua senha atual").ClickAsync();

            await Page.GetByPlaceholder("Digite sua senha atual").TypeAsync("Th0m45@K03n");

            var urlAtual = Page.Url;

            await Page.GetByLabel("Botão Entrar. Aperte a tecla enter para entrar.").ClickAsync();

            try
            {
                QuebrarCaptcha(urlAtual);
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
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

            // use to set invisible mode
            //api.IsInvisible = true

            // use to set Hcaptcha Enterprise parameters like rqdata, sentry, apiEndpoint, endpoint, reportapi, assethost, imghost
            //api.EnterprisePayload.Add("rqdata", "rqdata value from target website");
            //api.EnterprisePayload.Add("sentry", "true");

            if (!api.CreateTask())
                DebugHelper.Out("API v2 send failed. " + api.ErrorMessage, DebugHelper.Type.Error);
            else if (!api.WaitForResult())
                DebugHelper.Out("Could not solve the captcha.", DebugHelper.Type.Error);
            else
                DebugHelper.Out("Result: " + api.GetTaskSolution().GRecaptchaResponse, DebugHelper.Type.Success);
        }

    }
}
