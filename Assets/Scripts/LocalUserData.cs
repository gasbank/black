// BlackSaveData에 속한 자료형이므로 한번 추가한 필드는 절대로 빼면 안된다!
// (BinaryFormatter!!!)

using System;
using MessagePack;

[Serializable]
[MessagePackObject]
public class LocalUserData
{
    [Key(0)]
    public long createdNetworkUtcTicks;

    [Key(1)]
    public long lastUsedNetworkUtcTicks;

    [Key(2)]
    public string id;

    [Key(3)]
    public string name;

    public override string ToString()
    {
        return
            $"[Local User Data] ID:{id} Created:{new DateTime(createdNetworkUtcTicks, DateTimeKind.Utc)} Last:{new DateTime(lastUsedNetworkUtcTicks, DateTimeKind.Utc)} Name:{name}";
    }
}