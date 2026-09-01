using System.Collections.Concurrent;

namespace ProductManagement.API.Authentication
{
    public interface IHmacNonceStore
    {
        bool TryUseNonce(
            string clientId,
            string nonce,
            DateTimeOffset expiry);
    }

    public class HmacNonceStore : IHmacNonceStore
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> _nonces
            = new();

        public bool TryUseNonce(
            string clientId,
            string nonce,
            DateTimeOffset expiry)
        {
            CleanupExpiredNonces();

            string key = $"{clientId}:{nonce}";

            return _nonces.TryAdd(key, expiry);
        }

        private void CleanupExpiredNonces()
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var item in _nonces)
            {
                if (item.Value <= now)
                {
                    _nonces.TryRemove(
                        item.Key,
                        out _);
                }
            }
        }
    }
}