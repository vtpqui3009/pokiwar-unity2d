using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Pokiwar.Multiplayer
{
    /// <summary>
    /// In-game text chat with message history, player name prefix, and system messages.
    /// </summary>
    public class ChatSystem : NetworkBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;

        [Header("Settings")]
        [SerializeField] private int maxMessages = 20;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        private readonly List<string> messageHistory = new List<string>();
        private bool isChatOpen;

        private void Start()
        {
            if (sendButton != null)
                sendButton.onClick.AddListener(SendMessage);

            if (chatPanel != null)
                chatPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                ToggleChat();

            if (isChatOpen && Input.GetKeyDown(KeyCode.Return))
                SendMessage();
        }

        public void ToggleChat()
        {
            isChatOpen = !isChatOpen;
            chatPanel?.SetActive(isChatOpen);

            if (isChatOpen)
                inputField?.Select();
        }

        public void SendMessage()
        {
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text)) return;

            string message = inputField.text.Trim();
            inputField.text = string.Empty;

            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            string fullMessage = $"[{playerName}]: {message}";

            SendMessageServerRpc(fullMessage);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendMessageServerRpc(string message, ServerRpcParams rpcParams = default)
        {
            // Basic sanitization
            if (string.IsNullOrWhiteSpace(message) || message.Length > 200) return;

            BroadcastMessageClientRpc(message);
        }

        [ClientRpc]
        private void BroadcastMessageClientRpc(string message)
        {
            AddMessageToUI(message);
        }

        public void AddSystemMessage(string message)
        {
            string systemMsg = $"<color=#AAAAAA>[System] {message}</color>";
            AddMessageToUI(systemMsg);
        }

        private void AddMessageToUI(string message)
        {
            messageHistory.Add(message);
            if (messageHistory.Count > maxMessages)
                messageHistory.RemoveAt(0);

            if (messageContainer == null || messagePrefab == null) return;

            // Remove oldest if over limit
            if (messageContainer.childCount >= maxMessages)
                Destroy(messageContainer.GetChild(0).gameObject);

            GameObject msgObj = Instantiate(messagePrefab, messageContainer);
            TextMeshProUGUI text = msgObj.GetComponent<TextMeshProUGUI>();
            if (text != null)
                text.text = message;
        }

        public IReadOnlyList<string> GetMessageHistory() => messageHistory;
    }
}
