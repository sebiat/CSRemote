namespace CSWebInterface.HTTP.Sites
{
    internal class HTMLSite
    {
        private const string HTTPHeader = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\"\"http://www.w3.org/TR/html4/loose.dtd\">";

        private string _sourceCode;
        private readonly string _title;
        public HTMLSite(string aSiteTitle)
        {
            _sourceCode = HTTPHeader;
            _sourceCode += "\n<html>";
            _sourceCode += MakeHead(aSiteTitle);
            _title = aSiteTitle;
        }
        private static string MakeHead(string aTitle)
        {
           
            return "\n<head>\n<title>" + aTitle + "</title> </head>";
        }
        public void AddLink(string aText, string aLink)
        {
            _sourceCode += "<a href=" + aLink + " style=\"color:black\">" + aText + "</a>";
        }
        public void AddLink(string aText, string aLink, string aColor)
        {
            _sourceCode += "<a href=" + aLink + " style=\"color:" +aColor + "\">" + aText + "</a>";
        }

        public void AddText(string aText)
        {
            _sourceCode += aText;
        }

        public void AddButton(string aText, string aLink)
        {
            _sourceCode += "<input type=\"button\" name=" + aText +
                           " value=\"" + aText + "\" onClick=\"self.location.href='" + aLink + "'\">";
        }
        public void NextLine()
        {
            _sourceCode += "<br>\n";
        }
        public string PrintPage()
        {
            return _sourceCode + "\n</html>";
        }

        public void Clear()
        {
            _sourceCode = HTTPHeader;
            _sourceCode += "\n<html>";
            _sourceCode += MakeHead(_title);
        }
    }
}
