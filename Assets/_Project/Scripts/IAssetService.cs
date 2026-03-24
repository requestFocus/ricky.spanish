using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAssetService
{
    public UniTask<Sprite> LoadSprite(string key);
    
    public void ReleaseAsset(object sprite);
}