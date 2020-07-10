using MNepalAPI.Connection;
using MNepalAPI.Models;
using MNepalAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using ZXing;

namespace MNepalAPI.Controllers
{
    public class QRCodeGeneratorController : ApiController
    {
        [Route("api/QRCodeGenerator/Generate")]
        [HttpPost]
        public async Task<HttpResponseMessage> Generate([FromBody] QRCode qRCode)
        {
            QRCode code = new QRCode();
            var imageUrl = GenerateQRCode(qRCode.merchantName, qRCode.merchantId);
            code.merchantName = qRCode.merchantName;
            code.merchantId = qRCode.merchantId;
            code.qrCodeImagePath = imageUrl;
            return Request.CreateResponse(HttpStatusCode.OK, code);

        }

        public string GenerateQRCode(string merchantName, string merchantId)
        {
            string folderPath = "~/Images/";
            string error = "error";
            string imagePath = "~/Images/" + merchantId + ".jpg";
            // If the directory doesn't exist then create it.
            if (!Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath(folderPath)))
            {
                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath(folderPath));
            }
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            var result = barcodeWriter.Write(merchantName + Environment.NewLine + merchantId);

            string barcodePath = System.Web.Hosting.HostingEnvironment.MapPath(imagePath);
            var barcodeBitmap = new Bitmap(result);
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(barcodePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    barcodeBitmap.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            QRCode qRCode = new QRCode();
            qRCode.merchantName = merchantName;
            qRCode.merchantId = merchantId;
            qRCode.qrCodeImagePath = imagePath;

            //save imagePath to db
            int resultsPayments = QRCodeGeneratorUtilities.QRCode(qRCode);
            if (resultsPayments != 100)
            {
                return error;
            }
            //db end
            return imagePath;
        }

        [Route("api/QRCodeGenerator/ReadQRCode")]
        [HttpPost]
        public async Task<HttpResponseMessage> ReadQRCode(QRCode qRCode)
        {
            QRCodeReader qrCodeReader = new QRCodeReader();
            string qrCodeText = "";
            string qrCodeNumber = "";
            string imagePath = qRCode.qrCodeImagePath;
            string barcodePath = System.Web.Hosting.HostingEnvironment.MapPath(imagePath);
            var barcodeReader = new BarcodeReader();

            var result = barcodeReader.Decode(new Bitmap(barcodePath));
            if (result != null)
            {
                string[] lines = result.ToString().Split(new[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.None);
                qrCodeText = lines[0];
                qrCodeNumber = lines[1];
            }

            qrCodeReader.merchantName = qrCodeText;
            qrCodeReader.merchantId = qrCodeNumber;
            return Request.CreateResponse(HttpStatusCode.OK, qrCodeReader);

        }

        [Route("api/QRCodeGenerator/GetQRCodeImage")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetQRCodeImage(QRCode qRCode)
        {
            QRCode model = new QRCode();
            DataTable dtableUser = GetQRCodeImageUrl.GetQRCodeImage(qRCode.merchantName, qRCode.merchantId);
            if (dtableUser != null && dtableUser.Rows.Count > 0)
            {
                model.merchantName = dtableUser.Rows[0]["MerchantName"].ToString();
                model.merchantId = dtableUser.Rows[0]["MerchantId"].ToString();
                model.qrCodeImagePath = dtableUser.Rows[0]["QRCodeUrl"].ToString();
            }
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }
    }
}
