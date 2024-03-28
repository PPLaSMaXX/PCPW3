using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCPW3
{
    class Parser
    {
        private static readonly Regex sWhitespace = new Regex(@"\s+");

        public async Task<List<ParsedProduct>> ParseLink(string link)
        {
            if (!CheckInternetConnection(link)) return null;

            List<ParsedProduct> products = new List<ParsedProduct>();

            // Cheking link for correctness
            if (!ValidateLink(link))
            {
                MessageBox.Show("Error: Link is not valid or empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            IConfiguration config = Configuration.Default.WithDefaultLoader();

            // Loading page HTML code
            IDocument document = await BrowsingContext.New(config).OpenAsync(link);

            // Selecting all products in list
            IHtmlCollection<IElement> parsedProducts = document.QuerySelectorAll(".list-item--goods a[id^=product]");


            foreach (IHtmlAnchorElement productLink in parsedProducts)
            {
                // Loading product page HTML code
                IDocument productDocument = await BrowsingContext.New(config).OpenAsync(productLink.Href);

                // Selecting data with selectors
                IElement IFullName = productDocument.QuerySelector("h1.t2.no-mobile");
                IElement IPrice = productDocument.QuerySelector(".desc-big-price span:first-child");
                IElement IName = productDocument.QuerySelector("#top-page-title");

                // Getting Name from HTML
                string name = ReplaceWhitespace(IName.GetAttribute("data-txt-title"));

                string fullName = null;
                if (IFullName.Children[0].ClassName == "item-conf-name ib nobr" || IFullName.Children[0].ClassName == "apple-year")
                {
                    fullName = ReplaceWhitespace(IFullName.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault());
                }
                else
                {
                    fullName = IFullName.QuerySelector(".ib").ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
                }


                // Getting category from HTML
                string category = null;
                if (IFullName.Children[0].ClassName != "item-conf-name ib nobr" && IFullName.Children[0].ClassName != "apple-year")
                {
                    category = IFullName.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
                }
                else
                {
                    category = GetFirstWords(fullName, name);
                }

                products.Add(new ParsedProduct(category, RemoveSpace(IPrice.TextContent), name));
            }

            return products;
        }

        private string RemoveSpace(string input)
        {
            string result = null;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].IsDigit())
                {
                    result += input[i];
                }
            }
            return result;
        }

        public static string ReplaceWhitespace(string input)
        {
            return sWhitespace.Replace(input, " ");
        }

        private bool ValidateLink(string link)
        {
            return Uri.TryCreate(link, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static bool CheckInternetConnection(string link)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
                request.KeepAlive = false;
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }

        string GetFirstWords(string text1, string text2)
        {
            string[] words1 = text1.Split(); 
            string[] words2 = text2.Split(); 

            List<string> firstWords = new List<string>();

            for (int i = 0; i < words1.Length && i < words2.Length; i++)
            {
                foreach(string checking in words2)
                {
                    if (words1[i].Equals(checking, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Join(" ", firstWords);
                    }
                    else
                    {
                        firstWords.Add(words1[i]);
                        break;
                    }
                }
            }

            return string.Join(" ", " ");
        }
    }
}
