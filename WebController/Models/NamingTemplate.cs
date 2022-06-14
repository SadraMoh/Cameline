using System.Text.RegularExpressions;

namespace WebController.Models
{
    public class NamingTemplate
    {
        public string TemplateString { get; private set; }

        #region Tokens

        const string TOKENSTART = "{";
        const string TOKENEND = "}";

        const string BOOKNAME_TOKEN = "bookname";
        const string CAMERANAME_TOKEN = "camera";
        const char NUM_TOKEN = 'n';

        #endregion

        public NamingTemplate(string templateString)
        {
            if (!Validate(templateString))
                throw new ArgumentNullException(nameof(templateString), "the provided template string was not in the correct format");

            TemplateString = templateString;
        }

        public string Generate(string bookName = "[bookname]", string cameraName = "[cameraname]", int number = 0)
        {
            string ans = TemplateString;

            const string bookNameKey = TOKENSTART + BOOKNAME_TOKEN + TOKENEND;
            ans = ans.Replace(bookNameKey, bookName);

            const string cameraNameKey = TOKENSTART + CAMERANAME_TOKEN + TOKENEND;
            ans = ans.Replace(cameraNameKey, cameraName);

            foreach (Match match in Regex.Matches(ans, "{(" + NUM_TOKEN + ".*?)}"))
            {
                // match.Value == "{nnn}"

                // find how many nnn there are
                int nCount = match.Value.Count(c => c == NUM_TOKEN);

                // generate padded string
                string interpreted = number.ToString().PadLeft(nCount, '0');

                ans = ans.Replace(match.Value, interpreted);
            }

            return ans;
        }

        static bool Validate(string subject)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => TemplateString;

    }
}
