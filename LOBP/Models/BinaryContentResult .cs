using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LOBP.Models
{
    public class BinaryContentResult : ActionResult
    {

        private readonly string _contentType;
        private readonly byte[] _contentBytes;
        private readonly string filename;

        public BinaryContentResult(byte[] contentBytes, string contentType,string filename)
        {
            this._contentBytes = contentBytes;
            this._contentType = contentType;
            this.filename = filename;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentType = this._contentType;
            //response.AddHeader("content-disposition",  "attachment; filename="+filename+".pdf");
            //response.AddHeader("content-disposition",  "attachment; filename="+filename+".pdf");
           

            using (var stream = new MemoryStream(this._contentBytes))
            {
                stream.WriteTo(response.OutputStream);
                stream.Flush();
            }
        }
    }
}