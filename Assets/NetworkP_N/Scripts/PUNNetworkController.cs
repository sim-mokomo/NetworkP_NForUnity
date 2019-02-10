using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PUNNetworkController : Photon.MonoBehaviour
{
    [SerializeField] private Text connectConditionHud;
    [SerializeField] private TitleHudController _titleHudController;
    [SerializeField] private P_NGameController _pNGameController;
    [SerializeField] private MatchingLoadHudController _matchingLoadHudController;

    private List<RoomInfo> _currentRoomInfos;
    private bool[] _createdRoomNumberList = new bool[100];

    private void Start()
    {
        PhotonNetwork.autoJoinLobby = true;
        bool successToConnect = PhotonNetwork.ConnectUsingSettings(gameVersion: "1.0v");
        if (successToConnect == false)
        {
            Debug.Log("Server connection failure");
            connectConditionHud.text = "Server connection failure";
        }

        _currentRoomInfos = new List<RoomInfo>();
        _matchingLoadHudController.gameObject.SetActive(false);
        _titleHudController.gameObject.SetActive(true);
        _titleHudController.JoinGameButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.connectedAndReady)
            {
                JoinGame(_currentRoomInfos);
            }
        });
        _pNGameController.OnBattleStart += () => { _matchingLoadHudController.gameObject.SetActive(false); };
    }

    // 1.ロビーに入室した時に全Roomの状況を検索する
    // 2.ルームの中に入室人数が1のものがあればそこに参加する
    // 3.以上のようなルームが存在しない場合は自分でルームを作成して入室する。

    public virtual void OnReceivedRoomListUpdate()
    {
        RoomInfo[] roomInfo = PhotonNetwork.GetRoomList();
        _currentRoomInfos = roomInfo.ToList();

        Debug.Log($"更新! | Room 個数{_currentRoomInfos.Count}");
        // 未使用の部屋を解放するお
        _currentRoomInfos.ForEach(ri =>
        {
            Debug.Log($"部屋名:{ri.name}");
            Debug.Log($"入室者:{ri.PlayerCount}/{ri.MaxPlayers}");
        });

        _createdRoomNumberList
            .Select((c, i) => new {created = c, index = i})
            .ToList()
            .ForEach(info =>
            {
                string confirmRoomName = $"Room_{info.index}";
                bool anyOneJoinRoom = _currentRoomInfos
                                          .FirstOrDefault(r => r.name.Equals(confirmRoomName)) != null;
                _createdRoomNumberList[info.index] = anyOneJoinRoom;
            });
    }

    // 未使用の部屋番号を取得
    public int GetUnusedRoomNumber()
    {
        int unusedIndex = -1;
        unusedIndex = _createdRoomNumberList.ToList().FindIndex(r => r == false);
        return unusedIndex;
    }

    public void OnLeftRoom()
    {
        Debug.Log("On Left Room");
        _titleHudController.gameObject.SetActive(true);
    }

    public void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        connectConditionHud.text = "Joined Lobby";
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        connectConditionHud.text = "Joined Room";
    }

    // ゲームに参加
    private void JoinGame(List<RoomInfo> roomInfos)
    {
        // 個々のRoomの名前を表示.
        RoomInfo joinRoomInfo = null;
        foreach (var info in roomInfos)
        {
            Debug.Log($"部屋名:{info.name}");
            Debug.Log($"入室者:{info.PlayerCount}/{info.MaxPlayers}");
            bool waitPlayer = (bool) info.CustomProperties["WaitPlayer"];
            Debug.Log($"相手を待っているか:{waitPlayer}");
            if (info.PlayerCount == 1 && waitPlayer)
            {
                int roomNumber = int.Parse(info.name.Split('_')[1]);
                joinRoomInfo = info;
            }
        }

        //ルームが存在しない場合は自分で作成する。
        if (joinRoomInfo == null)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            roomOptions.CustomRoomProperties =
                new ExitGames.Client.Photon.Hashtable() {{"WaitPlayer", true}};
            roomOptions.CustomRoomPropertiesForLobby = new string[] {"WaitPlayer"};

            int unusedRoomNumber = GetUnusedRoomNumber();
            PhotonNetwork.JoinOrCreateRoom(roomName: $"Room_{unusedRoomNumber}",
                roomOptions: roomOptions,
                typedLobby: TypedLobby.Default);
            Debug.Log($"部屋作成 Room_{unusedRoomNumber}");
            _createdRoomNumberList[unusedRoomNumber] = true;
        }
        else
        {
            // 誰かが待っていればそこに参加
            int roomNumber = int.Parse(joinRoomInfo.name.Split('_')[1]);
            Debug.Log($"Join {joinRoomInfo}");
            PhotonNetwork.JoinRoom(roomName: joinRoomInfo.name);
        }

        _titleHudController.gameObject.SetActive(false);
        _matchingLoadHudController.gameObject.SetActive(true);
    }
}