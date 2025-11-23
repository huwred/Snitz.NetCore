using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MVCForum.Controllers
{
    public class PaypalController : SnitzBaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PaypalController(
            IMember memberService,
            ISnitzConfig config,
            IHtmlLocalizerFactory localizerFactory,
            SnitzDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory) 
            : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Donate()
        {
            return View();
        }
        public IActionResult Completed()
        {
            // Example: Read PayPal return parameters
            string transactionId = Request.Query["tx"];
            string paymentStatus = Request.Query["st"];
            string amount = Request.Query["amt"];

            // TODO: Validate transaction with PayPal API (recommended for security)
            // TODO: Update your database with the payment info

            // Pass data to the view if needed
            ViewData["TransactionId"] = transactionId;
            ViewData["PaymentStatus"] = paymentStatus;
            ViewData["Amount"] = amount;

            return View();
        }
        public IActionResult Receive()
        {
            IPNContext ipnContext = new IPNContext()
            {
                IPNRequest = Request
            };

            using (StreamReader reader = new StreamReader(ipnContext.IPNRequest.Body, Encoding.ASCII))
            {
                ipnContext.RequestBody = reader.ReadToEnd();
            }

            //Store the IPN received from PayPal
            LogRequest(ipnContext);

            // Fire and forget verification task using HttpClient
            Task.Run(() => VerifyTaskAsync(ipnContext));

            //Reply back a 200 code
            return Ok();
        }
        private class IPNContext
        {
            public HttpRequest IPNRequest { get; set; }

            public string RequestBody { get; set; }

            public string Verification { get; set; } = String.Empty;
        }
        private async Task VerifyTaskAsync(IPNContext ipnContext)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.sandbox.paypal.com/cgi-bin/webscr");
                request.Content = new StringContent("cmd=_notify-validate&" + ipnContext.RequestBody, Encoding.ASCII, "application/x-www-form-urlencoded");

                var response = await client.SendAsync(request);
                ipnContext.Verification = await response.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                //Capture exception for manual investigation
            }

            ProcessVerificationResponse(ipnContext);
        }


        private void LogRequest(IPNContext ipnContext)
        {
            // Persist the request values into a database or temporary data store
        }

        private void ProcessVerificationResponse(IPNContext ipnContext)
        {
            if (ipnContext.Verification.Equals("VERIFIED"))
            {
                // check that Payment_status=Completed
                // check that Txn_id has not been previously processed
                // check that Receiver_email is your Primary PayPal email
                // check that Payment_amount/Payment_currency are correct
                // process payment
            }
            else if (ipnContext.Verification.Equals("INVALID"))
            {
                //Log for manual investigation
            }
            else
            {
                //Log error
            }
        }
    }
}
