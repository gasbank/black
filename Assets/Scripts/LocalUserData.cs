// BlackSaveData2에 속한 자료형이므로 한번 추가한 필드는 절대로 빼면 안된다!
// (BinaryFormatter!!!)
using System;

[Serializable]
public class LocalUserData {
    public long createdNetworkUtcTicks;
    public long lastUsedNetworkUtcTicks;
    public string id;
    public string name;
    public override string ToString() {
        return $"[Local User Data] ID:{id} Created:{new DateTime(createdNetworkUtcTicks, DateTimeKind.Utc)} Last:{new DateTime(lastUsedNetworkUtcTicks, DateTimeKind.Utc)} Name:{name}";
    }
}