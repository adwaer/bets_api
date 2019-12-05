using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Bets.Selenium
{
    public static class DriverFactory
    {
        public static WebDriverWrap GetNewDriver(string driverType)
        {
            RemoteWebDriver driver;
            if (driverType.Equals("chrome", StringComparison.CurrentCultureIgnoreCase))
            {
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                driver = new ChromeDriver(options);
                //driver.Manage().Window.Size
            }
            else
            {
                driver = new FirefoxDriver();
            }

            return new WebDriverWrap(driver);
        }
    }
}