using System.Collections.Generic;

public class GameEventData
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public GameEventData Set(string key, object value)
    {
        data[key] = value;
        return this;
    }
    public T Get<T>(string key) => data.ContainsKey(key) ? (T)data[key] : default;
    public bool Has(string key) => data.ContainsKey(key);
}