namespace VotoTrack.Data
{
    public static class SupabaseClientProvider
    {
        private static Supabase.Client _client;

        public static void Initialize(string url, string key)
        {
            var options = new Supabase.SupabaseOptions { AutoConnectRealtime = true };
            _client = new Supabase.Client(url, key, options);
        }

        public static Supabase.Client Client => _client ?? throw new Exception("Supabase Client não inicializado.");
    }
}