// task-ui/src/UserPage.jsx

import { useEffect, useState } from "react";
import axios from "axios";
import "./UserPage.css";
import { API_BASE_URL } from './apiConfig'; // DÜZELTME 1: Merkezi API adresi import edildi.

function UserPage({ onBack }) {
  const [username, setUsername] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [oldPassword, setOldPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState("");
  const [ok, setOk] = useState("");

  const token = localStorage.getItem("token");

  // Tüm istekler için ortak header.
  const jsonHeaders = {
    Authorization: "Bearer " + token,
    "Content-Type": "application/json",
  };
  
  useEffect(() => {
    // DÜZELTME 2: Istek API Gateway üzerinden yapılıyor.
    axios
      .get(`${API_BASE_URL}/users/me`, { headers: jsonHeaders })
      .then((res) => setDisplayName(res.data.username))
      .catch(() => setDisplayName(""));
  }, []); // Boş bağımlılık dizisi, sadece component yüklendiğinde çalışmasını sağlar.

  const handleUsername = async () => {
    if (!username.trim()) {
        setError("Yeni kullanıcı adı boş olamaz.");
        return;
    }
    setError("");
    setOk("");

    try {
      // DÜZELTME 3: Istek API Gateway üzerinden ve doğru formatta (nesne olarak) yapılıyor.
      await axios.patch(
        `${API_BASE_URL}/users/username`,
        { newUsername: username }, // Backend'in beklediği DTO formatı
        { headers: jsonHeaders }
      );
      setOk("Kullanıcı adı başarıyla değiştirildi.");
      setDisplayName(username);
      setUsername("");
    } catch (err) {
      const apiError = err.response?.data?.message;
      setError(apiError || "Kullanıcı adı güncellenemedi.");
    }
  };

  const handlePassword = async () => {
    if (!oldPassword || !newPassword) {
        setError("Eski ve yeni şifre alanları boş olamaz.");
        return;
    }
    setError("");
    setOk("");
    
    try {
      // DÜZELTME 4: Istek API Gateway üzerinden yapılıyor.
      await axios.patch(
        `${API_BASE_URL}/users/password`,
        { oldPassword, newPassword },
        { headers: jsonHeaders }
      );
      setOk("Şifre başarıyla değiştirildi.");
      setOldPassword("");
      setNewPassword("");
    } catch (err) {
      const apiError = err.response?.data?.message;
      setError(apiError || "Şifre güncellenemedi. Eski şifrenizi doğru girdiğinizden emin olun.");
    }
  };

  const handleLogout = () => {
    localStorage.clear();
    window.location.reload();
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