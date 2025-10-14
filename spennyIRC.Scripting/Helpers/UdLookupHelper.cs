namespace spennyIRC.Scripting.Helpers
{
    public static class UdLookupHelper
    {
        public static async Task<string> UdLookupAsync(string ud)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://urbandictionary.com/") };

            var html = await (await httpClient.GetAsync($"define.php?term={ud}")).Content.ReadAsStringAsync();
            // todo:
            // parse this to break definition into groups
            // .definition (class name) - container of each definition
            // .meaning
            // .example
            return html;
        }
    }
}