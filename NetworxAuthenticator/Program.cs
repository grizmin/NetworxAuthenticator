using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace NetworxAuthenticator
{   
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(180, 60);
            Console.SetBufferSize(180, 999);
            Authenticator a = new Authenticator();
            a.Authenticate();
            if (a.challenge == null || a.challenge == "") {
                Console.ForegroundColor = System.ConsoleColor.Red;
                Console.WriteLine("Challenge could not be found.\nPress enter to exit.");
            }
            else
                Console.WriteLine("Authenticated. Press enter to exit.");
            Console.ReadLine();
        }

    }

    class Authenticator
    {
        public string username { get; set; }
        public string password { get; set; }
        public string url { get; set; }
        public string challenge { get; set; }


        public Authenticator()
        {
            this.ParseConfig();
        }

        private static string getPage(string url)
        {
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }
            
        public string GetChal()
        {
            string searchString = "name=\"chal\" value=\"(\\w+)\"";
            string chalPage = getPage(url);
            Match match = Regex.Match(chalPage, searchString);
            challenge = match.Groups[1].Value;
            return challenge;
        }

        public void Authenticate(string chal)
        {
            string authURL = string.Format(
                "https://wifi.networx-bg.com/hotspotlogin.php?chal={0}&uamip=172.16.0.1&uamport=3990&userurl={3}&provider=&username={1}&password={2}",
                chal, username, password,url
                );
            //OpenUri(testURL);
            string responsepage = getPage(authURL);
            string searchString = "content=\"0;url=(http://.*)\"";
            Match match = Regex.Match(responsepage, searchString);
            string authURI = match.Groups[1].Value;
            Console.WriteLine("Auth URI:  {0}", authURI);
            getPage(authURI);
            Console.WriteLine(getPage("http://web.grizmin.org/ip"));
        }

        public void Authenticate()
        {
            Console.WriteLine("Getting challenge.");
            GetChal();
            if (challenge == null || challenge == "")
            {
                Console.WriteLine("challenge is not found. I will try to authenticate again.");
                for (int i = 1; i < 6; i++)
                {
                    Console.WriteLine("C {1}, Attempt {0}/5", i, challenge);
                    if (challenge == null || challenge == "")
                    {
                        GetChal();
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        Authenticate(challenge);
                        break;
                    }
                }

            }
            else
            {
                string authURL = string.Format(
                    "https://wifi.networx-bg.com/hotspotlogin.php?chal={0}&uamip=172.16.0.1&uamport=3990&userurl={3}&provider=&username={1}&password={2}",
                    challenge, username, password, url
                    );
                //OpenUri(testURL);
                string responsepage = getPage(authURL);
                string searchString = "content=\"0;url=(http://.*)\"";
                Match match = Regex.Match(responsepage, searchString);
                string authURI = match.Groups[1].Value;
                Console.WriteLine("Challenge: {1} Auth URI:  {0}", authURI, challenge);
                getPage(authURI);
                Console.WriteLine(getPage("http://web.grizmin.org/ip"));

            }
            
        }

        private static bool IsValidUri(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                return false;
            Uri tmp;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
                return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        public static bool OpenUri(string uri)
        {
            if (!IsValidUri(uri))
                return false;
            System.Diagnostics.Process.Start(uri);
            return true;
        }

        private void ParseConfig()
        {
            string path = Directory.GetCurrentDirectory() + "\\config.txt";
            FileInfo config = new FileInfo(path);
            if (config.Exists)
            {
                using (StreamReader sr = new StreamReader(config.FullName))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("username"))
                        {
                            this.username = line.Substring(9).Trim();
                        }
                        if (line.StartsWith("password"))
                        {
                            this.password = line.Substring(9).Trim();
                        }
                        if (line.StartsWith("url"))
                        {
                            string p = line.Substring(4).Trim();
                            if (IsValidUri(p))
                            {
                                this.url = line.Substring(4).Trim();
                                //Console.WriteLine(url);
                            }
                            else
                            {
                                Console.WriteLine("URL IS MALFORMED!");
                                throw new Exception("url is malformed");

                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("config is missing");
            }
        }
    }
}