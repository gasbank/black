using UnityEngine;

public interface INetworkTimeSubscriber {
    GameObject gameObject { get; }
    void OnNetworkTimeStateChange(NetworkTime.QueryState state);
}
