using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PixelBender.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase photo, int borderRadius)
        {
            if (photo != null && photo.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(photo.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Photos/"), fileName);

                    // Save the uploaded photo to the server
                    photo.SaveAs(filePath);

                    // Apply border radius to the photo
                    string processedFileName = "FinalResult_" + fileName;
                    string processedFilePath = Path.Combine(Server.MapPath("~/Photos/"), processedFileName);
                    ApplyBorderRadius(filePath, processedFilePath, borderRadius);

                    // Delete the original uploaded photo
                    System.IO.File.Delete(filePath);

                    // Pass the processed photo file name to the view
                    ViewBag.ProcessedPhoto = processedFileName;

                    return View("Result");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "An error occurred: " + ex.Message;
                }
            }

            return View("Index");
        }

        public ActionResult DownloadPhoto(string fileName)
        {
            string processedFilePath = Path.Combine(Server.MapPath("~/Photos/"), fileName);

            byte[] fileBytes = System.IO.File.ReadAllBytes(processedFilePath);
            System.IO.File.Delete(processedFilePath); // Delete the processed photo

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private void ApplyBorderRadius(string sourceFilePath, string outputFilePath, int borderRadius)
        {
            using (Image sourceImage = Image.FromFile(sourceFilePath))
            {
                using (Bitmap roundedImage = ApplyBorderRadiusToImage(sourceImage, borderRadius))
                {
                    roundedImage.Save(outputFilePath);
                }
            }
        }

        private Bitmap ApplyBorderRadiusToImage(Image sourceImage, int borderRadius)
        {
            Bitmap roundedImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Graphics graphics = Graphics.FromImage(roundedImage);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, borderRadius, borderRadius, 180, 90); 
                path.AddArc(sourceImage.Width - borderRadius, 0, borderRadius, borderRadius, 270, 90); 
                path.AddArc(sourceImage.Width - borderRadius, sourceImage.Height - borderRadius, borderRadius, borderRadius, 0, 90);
                path.AddArc(0, sourceImage.Height - borderRadius, borderRadius, borderRadius, 90, 90); 

                graphics.Clip = new Region(path);
                graphics.DrawImage(sourceImage, Point.Empty);
            }

            return roundedImage;
        }

        public ActionResult Result()
        {
            return View();
        }

    }
}