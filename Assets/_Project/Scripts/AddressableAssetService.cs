using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableAssetService : IAssetService
{
    public async UniTask<Sprite> LoadSprite(string key)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(key);
        return await handle.ToUniTask();
    }

    public void ReleaseAsset(object asset)
    {
        if (asset != null)
        {
            Addressables.Release(asset);
        }
    }
}