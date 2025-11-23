using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

// 这个静态类为 UnityWebRequest 提供 await 支持
public static class UnityWebRequestAwaiter
{
    public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation req)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest.Result>();
        req.completed += op => tcs.TrySetResult(req.webRequest.result);
        return tcs.Task.GetAwaiter();
    }
}