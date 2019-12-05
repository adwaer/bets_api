﻿using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Bets.Selenium
{
    public class WebDriverWrap : IDisposable
    {
        private readonly IWebDriver _webDriver;
        private IWebElement _webElement;

        public WebDriverWrap(IWebDriver webDriver)
        {
            _webDriver = webDriver;
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);
            _webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(5);
            _webDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMinutes(5);
            try
            {
                _webDriver.Manage().Window.Maximize();
            }
            catch
            {
                // ignored
            }
        }

        public void Open(string url, By waitBy)
        {
            _webDriver.Navigate().GoToUrl(url);
            var link = _webDriver.FindElement(waitBy);
            var jsToBeExecuted = $"window.scroll(0, {link.Location.Y});";

            var javaScriptExecutor = (IJavaScriptExecutor) _webDriver;
            javaScriptExecutor.ExecuteScript(jsToBeExecuted);

            var wait = new WebDriverWait(_webDriver, TimeSpan.FromMinutes(1));
            _webElement = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(waitBy));
        }

        public IWebDriver GetPage()
        {
            return _webDriver;
        }

        public WebDriverWait GetWait(TimeSpan time)
        {
            return new WebDriverWait(_webDriver, time);
        }

        public IWebElement WaitClickable(TimeSpan time, By by)
        {
            return GetWait(time)
                .Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }

        public IWebElement GetPageEl()
        {
            return _webElement;
        }

        public void Dispose()
        {
            _webDriver?.Dispose();
        }
    }
}