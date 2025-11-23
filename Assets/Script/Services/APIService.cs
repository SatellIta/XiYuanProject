using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class APIService : MonoBehaviour
{
    public static APIService Instance { get; private set; }
    private const string BaseUrl = "http://1.117.207.132"; 

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // 泛型 POST 方法：不管是 Chat 还是 Confirm 都可以用
    public async Task<TResponse> PostAsync<TResponse>(string endpoint, object payload)
    {
        string url = $"{BaseUrl}{endpoint}";
        string jsonStr = JsonConvert.SerializeObject(payload);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonStr);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Error: {request.error} | {request.downloadHandler.text}");
                return default; // 或者抛出异常
            }

            string responseText = request.downloadHandler.text;
            // ★ DEBUG: 打印后端返回的原始 JSON，检查里面到底有没有 "problem" 字段
            Debug.Log($"[API Raw Response] {endpoint}: {responseText}");

            // ★ 关键修复：如果调用者想要 string，直接返回原始内容，不解析 JSON
            if (typeof(TResponse) == typeof(string))
            {
                return (TResponse)(object)responseText;
            }

            // 否则尝试解析 JSON
            try 
            {
                return JsonConvert.DeserializeObject<TResponse>(responseText);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON Parsing Error: {ex.Message}. Raw Text: {responseText}");
                throw;
            }
        }
    }
}