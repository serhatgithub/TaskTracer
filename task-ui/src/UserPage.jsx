import { useEffect, useState } from "react";
import axios from "axios";
import "./UserPage.css";

function UserPage({ onBack }) {
  const [username, setUsername] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [oldPassword, setOldPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [currentUsername, setCurrentUsername] = useState("");
  const [error, setError] = useState("");
  const [ok, setOk] = useState("");

  const headers = {
    Authorization: "Bearer " + localStorage.getItem("token"),
    "Content-Type": "application/json",
  };

  useEffect(() => {
    axios
      .get("http://localhost:5242/api/v1/users/me", { headers })
      .then((res) => setDisplayName(res.data.username))
      .catch(() => setDisplayName(""));
  }, []);

  const handleUsername = async () => {
    try {
      await axios.patch(
        "http://localhost:5242/api/v1/users/username",
        JSON.stringify(username),
        { headers }
      );
      setOk("Kullanıcı adı değiştirildi.");
      setError("");
      setDisplayName(username);
      setUsername("");
    } catch {
      setError("Kullanıcı adı güncellenemedi.");
      setOk("");
    }
  };

  const handlePassword = async () => {
    try {
      await axios.patch(
        "http://localhost:5242/api/v1/users/password",
        { oldPassword, newPassword },
        { headers }
      );
      setOk("Şifre değiştirildi.");
      setError("");
      setOldPassword("");
      setNewPassword("");
    } catch {
      setError("Şifre güncellenemedi.");
      setOk("");
    }
  };

  const handleLogout = () => {
    localStorage.clear();
    window.location.reload(); // Sayfayı yenileyerek login ekranına döner
  };

  return (
    <div className="user-page">
      <h2>Kullanıcı Paneli</h2>
      <p className="welcome-text">👋 Hoş geldin, {displayName}</p>

      <div className="input-group">
        <input
          placeholder="Yeni kullanıcı adı"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <button onClick={handleUsername}>Kullanıcı Adı Değiştir</button>
      </div>

      <div className="input-group">
        <input
          type="password"
          placeholder="Eski şifre"
          value={oldPassword}
          onChange={(e) => setOldPassword(e.target.value)}
        />
        <input
          type="password"
          placeholder="Yeni şifre"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
        />
        <button onClick={handlePassword}>Şifre Değiştir</button>
      </div>

      {error && <p className="error">{error}</p>}
      {ok && <p className="success">{ok}</p>}

      <div className="button-group">
        <button onClick={onBack} className="back-button">← Geri</button>
        <button onClick={handleLogout} className="logout-button">🚪 Çıkış Yap</button>
      </div>
    </div>
  );
}

export default UserPage;
