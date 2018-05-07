using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;

namespace CaseDownloader
{
    class DocumentWriter
    {

        WordprocessingDocument document;
        Body doc_body;

        public DocumentWriter(string path)
        {
            document = WordprocessingDocument.Create(path+".docx", WordprocessingDocumentType.Document);
            document.AddMainDocumentPart();
            document.MainDocumentPart.Document = new Document();
            doc_body = new Body();
        }

        public void addHeading(string heading)
        {
            var r1 = new Run();
            var rp1 = new RunProperties();
            rp1.Bold = new Bold();
            rp1.FontSize = new FontSize();
            rp1.FontSize.Val = 32.ToString();
            var head_text = new Text(heading);
            r1.Append(rp1);
            r1.Append(head_text);
            var p = new Paragraph(r1);
            doc_body.Append(p);
        }

        public void addText(string text)
        {
            var r1 = new Run();
            var rp1 = new RunProperties();
            rp1.Bold = new Bold();
            rp1.FontSize = new FontSize();
            rp1.FontSize.Val = 16.ToString();
            var head_text = new Text(text);
            r1.Append(rp1);
            r1.Append(head_text);
            var p = new Paragraph(r1);
            doc_body.Append(p);
        }

        public void Save()
        {
            document.MainDocumentPart.Document.Append(doc_body);
            document.Save();
            document.Close();
        }
    }
}
