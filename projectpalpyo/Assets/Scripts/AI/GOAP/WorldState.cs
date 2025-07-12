using System.Collections.Generic;

public class WorldState
{
    private Dictionary<string, bool> states;
    
    public WorldState()
    {
        states = new Dictionary<string, bool>();
    }
    
    public WorldState(WorldState other)
    {
        states = new Dictionary<string, bool>(other.states);
    }
    
    public void SetState(string key, bool value)
    {
        states[key] = value;
    }
    
    public bool GetState(string key)
    {
        return states.ContainsKey(key) ? states[key] : false;
    }
    
    public bool HasState(string key)
    {
        return states.ContainsKey(key);
    }
}