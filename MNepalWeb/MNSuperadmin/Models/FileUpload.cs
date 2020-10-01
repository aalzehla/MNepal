using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MNSuperadmin.Models
{
    public class FileUpload
    {
        public string ErrorMessage { get; set; }
        public decimal filesize { get; set; }
        public string UploadUserFile(HttpPostedFileBase file, string message)
        {
            try
            {
                //var supportedTypes = new[] { "txt", "doc", "docx", "pdf", "xls", "xlsx" };
                var supportedTypes = new[] { "xls", "xlsx" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt))
                {
                    ErrorMessage = "File Extension Is InValid - Only Upload EXCEL File";
                    return ErrorMessage;
                }
                else if(file.ContentLength > (filesize * 1024 * 1024))
                {
                    ErrorMessage = "File size Should Be UpTo " + filesize + "MB";
                    return ErrorMessage;
                }
                else if (message == "")
                {
                    ErrorMessage = "Message is required";
                    return ErrorMessage;
                }
                else
                {
                    return ErrorMessage;
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = "Upload Container Should Not Be Empty";
                return ErrorMessage;
            }
        }
    }
}