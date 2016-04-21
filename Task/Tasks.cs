using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    public static class Tasks
    {
        /// <summary>
        /// Returns the content of required uri's.
        /// Method has to use the synchronous way and can be used to compare the
        ///  performace of sync/async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris)
        {
            IEnumerable<string> resultContent = new List<string>();
            foreach (var uri in uris)
            {
                var webRequest = WebRequest.Create(uri);
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                    if (content != null)
                        using (var reader = new StreamReader(content))
                        {
                            var strContent = reader.ReadToEnd();
                            ((List<string>) resultContent).Add(strContent);
                        }
                    else
                    {
                        ((List<string>)resultContent).Add(uri + "fail with geting content");
                    }
            }
            return resultContent;
        }

        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace 
        /// of sync \ async approaches. 
        /// maxConcurrentStreams parameter should control the maximum of concurrent streams 
        /// that are running at the same time (throttling). 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            return RunTasksThatGetContent(uris, maxConcurrentStreams).GetAwaiter().GetResult();
        }

        private static async Task<IEnumerable<string>> RunTasksThatGetContent(IEnumerable<Uri> uris, int throttle)
        {
            IEnumerable<string> resultContent = new List<string>();
            var tasks = new List<Task<string>>();
            for (var n = 0; n < uris.Count(); n++)
            {
                var task = GettingContentForUri(uris.ToArray()[n]);
                tasks.Add(task);
                if (tasks.Count == throttle)
                {
                    await System.Threading.Tasks.Task.WhenAll(tasks);
                    foreach (var completed in tasks)
                    {
                        ((List<string>)resultContent).Add(completed.Result);
                    }
                    tasks.Clear();
                }
            }
            await System.Threading.Tasks.Task.WhenAll(tasks);
            foreach (var completed in tasks)
            {
                ((List<string>)resultContent).Add(completed.Result);
            }
            return resultContent;
        }

        private static Task<string> GettingContentForUri(Uri uri)
        {
            return Task<string>.Run(() =>
            {
                var webRequest = WebRequest.Create(uri);
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                    if (content != null)
                        using (var reader = new StreamReader(content))
                        {
                            var strContent = reader.ReadToEnd();
                            return strContent;
                        }
                    else
                    {
                        return "fail with geting content";
                    }
            });
        }

       // private Task<IEnumerable<string>> 

        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static Task<string> GetMD5Async(this Uri resource)
        {
            // TODO : Implement GetMD5Async
            throw new NotImplementedException();
        }
    }
}
