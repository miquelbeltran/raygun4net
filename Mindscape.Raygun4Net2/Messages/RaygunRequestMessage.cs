﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace Mindscape.Raygun4Net.Messages
{
  public class RaygunRequestMessage
  {
    public RaygunRequestMessage(HttpRequest request, List<string> ignoredFormNames)
    {
      HostName = request.Url.Host;
      Url = request.Url.AbsolutePath;
      HttpMethod = request.RequestType;
      IPAddress = request.UserHostAddress;
      IEnumerable<string> empty = new List<string>();
      QueryString = ToDictionary(request.QueryString, empty);

      Headers = ToDictionary(request.Headers, ignoredFormNames ?? empty);
      Headers.Remove("Cookie");

      Form = ToDictionary(request.Form, ignoredFormNames ?? empty, true);
      Cookies = GetCookies(request.Cookies, ignoredFormNames ?? empty);

      // Remove ignored and duplicated variables
      Data = ToDictionary(request.ServerVariables, ignoredFormNames ?? empty);
      Data.Remove("ALL_HTTP");
      Data.Remove("HTTP_COOKIE");
      Data.Remove("ALL_RAW");

      try
      {
        var contentType = request.Headers["Content-Type"];
        if (contentType != "text/html" && contentType != "application/x-www-form-urlencoded" && request.RequestType != "GET")
        {
          int length = 4096;
          string temp = new StreamReader(request.InputStream).ReadToEnd();
          if (length > temp.Length)
          {
            length = temp.Length;
          }

          RawData = temp.Substring(0, length);
        }
      }
      catch (HttpException)
      {
      }
    }

    private IDictionary GetCookies(HttpCookieCollection cookieCollection, IEnumerable<string> ignoredFormNames)
    {
      Dictionary<string, string> ignored = new Dictionary<string, string>();
      foreach(string key in ignoredFormNames)
      {
        ignored[key] = key;
      }

      IDictionary cookies = new Dictionary<string, string>();

      foreach (string key in cookieCollection.Keys)
      {
        if (!ignored.ContainsKey(key))
        {
          cookies[key] = cookieCollection[key].Value;
        }
      }

      return cookies;
    }

    private static IDictionary ToDictionary(NameValueCollection nameValueCollection, IEnumerable<string> ignoreFields, bool truncateValues = false)
    {
      Dictionary<string, string> ignored = new Dictionary<string, string>();
      foreach (string key in ignoreFields)
      {
        ignored[key] = key;
      }

      List<string> keys = new List<string>();

      try
      {
        foreach (string key in nameValueCollection)
        {
          if (!ignored.ContainsKey(key))
          {
            keys.Add(key);
          }
        }
      }
      catch (HttpRequestValidationException)
      {
        return new Dictionary<string, string> { { "Values", "Not able to be retrieved" } };
      }

      var dictionary = new Dictionary<string, string>();

      foreach (string key in keys)
      {
        try
        {
          var keyToSend = key;
          var valueToSend = nameValueCollection[key];

          if (truncateValues)
          {
            if (keyToSend.Length > 256)
            {
              keyToSend = keyToSend.Substring(0, 256);
            }

            if (valueToSend != null && valueToSend.Length > 256)
            {
              valueToSend = valueToSend.Substring(0, 256);
            }
          }

          dictionary.Add(keyToSend, valueToSend);
        }
        catch (HttpRequestValidationException e)
        {
          // If changing QueryString to be of type string in future, will need to account for possible
          // illegal values - in this case it is contained at the end of e.Message along with an error message

          int firstInstance = e.Message.IndexOf('\"');
          int lastInstance = e.Message.LastIndexOf('\"');

          if (firstInstance != -1 && lastInstance != -1)
          {
            dictionary.Add(key, e.Message.Substring(firstInstance + 1, lastInstance - firstInstance - 1));
          }
          else
          {
            dictionary.Add(key, string.Empty);
          }
        }
      }

      return dictionary;
    }

    public string HostName { get; set; }

    public string Url { get; set; }

    public string HttpMethod { get; set; }

    public string IPAddress { get; set; }

    public IDictionary QueryString { get; set; }

    public IDictionary Cookies { get; set; }

    public IDictionary Data { get; set; }

    public IDictionary Form { get; set; }

    public string RawData { get; set; }

    public IDictionary Headers { get; set; }

  }
}