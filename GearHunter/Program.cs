using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace GearHunter
{
    class Program
    {
            private static void DriverManager(IWebDriver driver)
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            }
            
            private static void SendEmail()
            {
            //BEGIN MAIL DINGES

            //string credentialGebruikersnaam = "csharpemailer@gmail.com";
            //string credentialWachtwoord = "Watisdeze1213";
            //string smtp = "smtp.gmail.com";
            //string inhoudMail = "Er is een nieuwe advertentie geplaatst!\n De link naar de advertentie is: dinges";

            try
            {
                var mail = new MailMessage();
                var smtpServer = new SmtpClient("smtp.gmail.com", 587);
                mail.From = new MailAddress("csharpemailer@gmail.com", "Tester.Email");
                mail.To.Add("csharpemailer@gmail.com");
                mail.Subject = "EMAIL ONDERWERP";
                mail.Body = "123456789 10.";
                smtpServer.Credentials = new NetworkCredential("csharpemailer@gmail.com", "Watisdeze1213");
                smtpServer.EnableSsl = true;
                smtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mail niet verstuurd: ");
                Console.WriteLine(ex);
            }
        }

            private static string SelectKeyWord(int CurrentKeyword)
            {
                SqlConnection conn = new SqlConnection("Data Source = DESKTOP-MOM6ULT; Initial Catalog = GearHunter; Integrated Security = True");
                conn.Open();

                SqlCommand command = new SqlCommand("SELECT Keyword FROM Keywords WHERE Id = '" + CurrentKeyword + "'", conn);
            
                string SearchWord = (string)command.ExecuteScalar();
                Console.WriteLine("Huidige Zoekterm: " + SearchWord);    

                conn.Close();
                return SearchWord;
            }

            private static int SelectKeyWordAmount()
            {
                SqlConnection conn = new SqlConnection("Data Source = DESKTOP-MOM6ULT; Initial Catalog = GearHunter; Integrated Security = True");
                conn.Open();

                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Keywords", conn);
                int Count = (int)command.ExecuteScalar();
                Console.WriteLine("Aantal Zoektermen: " + Count);  

                conn.Close();
                return Count;
            }
            
            static void Main(string[] args)
            {
                int KeywordsAmount = SelectKeyWordAmount();
                bool Running = true;
                while (Running == true)
                {
                    int CurrentKeyword = 1;
                    
                    while (CurrentKeyword <= KeywordsAmount)
                        {
                        string SearchWord = SelectKeyWord(CurrentKeyword);

                        IWebDriver driver = new ChromeDriver();
                        driver.Manage().Window.Maximize();

                        //open chrome web driver
                        driver.Navigate().GoToUrl("https://reverb.com");
                        DriverManager(driver);
                        IWebElement element = driver.FindElement(By.ClassName("site-search__controls__input"));

                        //Enter what you want to search for

                        element.SendKeys(SearchWord);
                        element.SendKeys(Keys.Enter);
                        DriverManager(driver);

                        //Switching to react modal
                    
                        driver.SwitchTo().ActiveElement();
                        IWebElement element1 = driver.FindElement(By.Id("user[first_name]"));
                        element1.SendKeys(Keys.Escape);

                        //Getting the Number of listings
                        
                        string listno = driver.FindElement(By.XPath("/html/body/div[3]/section/div[2]/div/div/div/div[2]/div/div[2]/nav/div[1]/h2/div")).Text.ToString();
                        listno.Replace(" Listings", "");
                        //int listnoint = Convert.ToInt32(listno);
                        Console.WriteLine("Klaar met zoekwoord no#: "+ CurrentKeyword.ToString());
                        Console.WriteLine("Aantal aangetroffen listings: " + listno.ToString());
                    
                        CurrentKeyword = CurrentKeyword + 1;
                        Console.WriteLine(CurrentKeyword);
                        driver.Close();

                        //if(CurrentKeyword == KeywordsAmount)
                    //{
                        //Running = false;
                    //}
                }
            }
         }
    }
}

