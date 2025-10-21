using System;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; 

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance; 
    string url = "http://127.0.0.1:1234";
    string token;
    string username;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public GameObject startPanel; 
    public GameObject authPanel; 
    public GameObject errorPanel; 
    public GameObject leaderboardPanel;
    public TextMeshProUGUI[] usernamesText;
    public TextMeshProUGUI[] scoresText;
    public Button leaderboardButton;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        token = PlayerPrefs.GetString("token");
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
        {
            Debug.Log("No hay token almacenado.");
            ShowAuthPanel();
        }
        else
        {
            Debug.Log("Token almacenado: " + token);
            Debug.Log("Usuario almacenado: " + username);
            StartCoroutine(GetProfile());
        }

         if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardButtonClicked);
        }
        else
        {
            Debug.LogError("Botón del leaderboard no asignado en el Inspector.");
        }
    }

    private void OnLeaderboardButtonClicked()
    {
        ShowLeaderboard(); 
    }

    public void Login()
    {
        Credentials credentials = new Credentials
        {
            username = usernameInput.text,
            password = passwordInput.text
        };
        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine(LoginPost(postData));
    }

    public void Register()
    {
        Credentials credentials = new Credentials
        {
            username = usernameInput.text,
            password = passwordInput.text
        };
        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine(RegisterPost(postData));
    }

    IEnumerator RegisterPost(string postData)
    {
        string path = "/api/usuarios";
        UnityWebRequest request = new UnityWebRequest(url + path, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Registro exitoso.");
            StartCoroutine(LoginPost(postData)); 
        }
        else
        {
            string errorMessage = "Error en registro: ";
            switch (request.responseCode)
            {
                case 400:
                    errorMessage += "Datos inválidos o faltantes.";
                    break;
                case 409:
                    errorMessage += "El nombre de usuario ya está en uso.";
                    break;
                case 500:
                    errorMessage += "Error interno del servidor.";
                    break;
                default:
                    errorMessage += "Error desconocido.";
                    break;
            }
            Debug.LogError(errorMessage);
            ShowError(); 
        }
    }

    IEnumerator LoginPost(string postData)
    {
        string path = "/api/auth/login";
        UnityWebRequest request = new UnityWebRequest(url + path, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);
            PlayerPrefs.SetString("token", response.token);
            PlayerPrefs.SetString("username", response.usuario.username);
            token = response.token;
            username = response.usuario.username;
            Debug.Log("Login exitoso!");
            ShowStartPanel(); 
        }
        else
        {
            string errorMessage = "Error en login: ";
            switch (request.responseCode)
            {
                case 400:
                    errorMessage += "Datos inválidos o faltantes.";
                    break;
                case 401:
                    errorMessage += "Usuario o contraseña incorrectos.";
                    break;
                case 500:
                    errorMessage += "Error interno del servidor.";
                    break;
                default:
                    errorMessage += "Error desconocido.";
                    break;
            }
            Debug.LogError(request.downloadHandler.text);
            Debug.LogError(request.responseCode);
            ShowError(); 
        }
    }

    IEnumerator GetProfile()
    {
        string path = "/api/usuarios";
        UnityWebRequest request = UnityWebRequest.Get(url + path);
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Usuario autenticado correctamente.");
        }
        else
        {
            string errorMessage = "Token inválido, redirigiendo a login.";
            Debug.LogError(errorMessage);
            ShowError();
            PlayerPrefs.DeleteKey("token");
            PlayerPrefs.DeleteKey("username");
        }
    }

    public void SaveAndSendScore(int score)
    {
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.Save();

        UserModel user = new UserModel
        {
            username = PlayerPrefs.GetString("username"),
            data = new DataUser { score = score }
        };

        // Convertir a JSON
        string jsonData = JsonUtility.ToJson(user);

        // Enviar el score a la API
        StartCoroutine(SendScoreToAPI(jsonData));
    }

    private IEnumerator SendScoreToAPI(string jsonData)
    {
        string path = "/api/usuarios";
        UnityWebRequest request = UnityWebRequest.Put(url + path, jsonData);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", PlayerPrefs.GetString("token")); // Enviar el token de autenticación

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Score enviado correctamente a la API.");
        }
        else
        {
            Debug.LogError("Error al enviar el score: " + request.error);
        }
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        StartCoroutine(GetLeaderboard());
    }

    private IEnumerator GetLeaderboard()
    {
        string path = "/api/usuarios";
        UnityWebRequest request = UnityWebRequest.Get(url + path);
        request.SetRequestHeader("x-token", PlayerPrefs.GetString("token")); // Enviar el token de autenticación

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Deserializar la respuesta de la API
            Debug.Log(request.downloadHandler.text);
            UsuariosResponse data = JsonUtility.FromJson<UsuariosResponse>(request.downloadHandler.text);

            // Ordenar los usuarios por score (de mayor a menor) y tomar los top 5
            var topUsers = data.usuarios.OrderByDescending(user => user.data.score).Take(5).ToArray();

            // Mostrar los top 5 scores en el panel del scoreboard
            DisplayLeaderboard(topUsers);
        }
        else
        {
            Debug.LogError("Error al obtener el leaderboard: " + request.error);
        }
    }

    private void DisplayLeaderboard(UserModel[] topUsers)
    {
        // Mostrar los nombres y scores en los TextMeshProUGUI correspondientes
        for (int i = 0; i < topUsers.Length; i++)
        {
            usernamesText[i].text = topUsers[i].username;
            scoresText[i].text = topUsers[i].data.score.ToString();
        }

        // Si hay menos de 5 usuarios, limpiar los campos restantes
        for (int i = topUsers.Length; i < 5; i++)
        {
            usernamesText[i].text = "";
            scoresText[i].text = "";
        }
    }

    public void ShowStartPanel()
    {
        startPanel.SetActive(true);
        authPanel.SetActive(false);
        errorPanel.SetActive(false);
    }

    public void ShowAuthPanel()
    {
        startPanel.SetActive(false);
        authPanel.SetActive(true);
        errorPanel.SetActive(false);
    }

    public void ShowError()
    {
        errorPanel.SetActive(true);
        authPanel.SetActive(true);
        startPanel.SetActive(false);
    }

    [Serializable]
    public class Credentials
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class AuthResponse
    {
        public UserModel usuario;
        public string token;
    }

    [Serializable]
    public class UserModel
    {
        public string _id;
        public string username;
        public DataUser data;
        public string estado;
    }

    [Serializable]
    public class DataUser
    {
        public int score;
    }

    [Serializable]
    public class UsuariosResponse
    {
        public List<UserModel> usuarios;
    }
}