// task-ui/src/LoginPage.jsx
import { useState } from "react";
import axios from "axios";
import "./LoginPage.css";
import { API_BASE_URL } from './apiConfig'; // API_BASE_URL'i import ediyoruz

function LoginPage({ onLogin }) {
  const [isRegistering, setIsRegistering] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!username.trim() || !password) {
      setError("Kullanıcı adı ve şifre boş bırakılamaz.");
      return;
    }

    // URL'ler API Gateway üzerinden olacak şekilde güncellendi
    const url = isRegistering
      ? `${API_BASE_URL}/auth/register`
      : `${API_BASE_URL}/auth/login`;

    try {
      const res = await axios.post(url, { username, password });

      if (isRegistering) {
        alert("Kayıt başarılı! Şimdi giriş yapabilirsin.");
        setIsRegistering(false);
        setUsername(""); // Başarılı kayıttan sonra inputları temizleyelim
        setPassword(""); // Başarılı kayıttan sonra inputları temizleyelim
        return;
      }

      localStorage.setItem("token", res.data.token);
      onLogin();
    } catch (err) {
      // API'lerimizden dönen hata mesajı yapısını kontrol edelim.
      // Bizim AuthController'da { message: "..." } şeklinde dönüyorduk.
      const apiErrorMessage = err.response?.data?.message; 
      const status = err.response?.status;

      console.error("Login/Register Error Response:", err.response); // Hata ayıklama için log

      if (isRegistering && status === 400 && apiErrorMessage?.toLowerCase().includes("username already exists")) {
        setError("Bu kullanıcı adı zaten kayıtlı.");
      } else if (!isRegistering && status === 401 && apiErrorMessage?.toLowerCase().includes("invalid username or password")) {
        setError("Kullanıcı adı veya şifre hatalı.");
      } else {
        setError(apiErrorMessage || "Bir hata oluştu. Lütfen tekrar deneyin.");
      }
    }
  };

  return (
    <div className="container">
      <div className="info-box">
        <h3>Hoş Geldiniz 👋</h3>
        <p>
          Bu uygulama sayesinde görevlerinizi kolayca takip edebilirsiniz.<br />
          Giriş yaptıktan sonra görev ekleyebilir, güncelleyebilir ve takip edebilirsiniz.
        </p>
      </div>

      <div className="inner-box">
        <div className="header">
          <h2 style={{ color: "#691bf1" }}>
            {isRegistering ? "Kayıt Ol" : "Giriş Yap"}
          </h2>
          <button
            className="register-btn"
            type="button"
            onClick={() => {
                setIsRegistering(!isRegistering);
                setError(""); // Mod değiştirirken hatayı temizle
                setUsername(""); // Mod değiştirirken inputları temizle
                setPassword(""); // Mod değiştirirken inputları temizle
            }}
          >
            {isRegistering ? "Giriş" : "Kayıt"}
          </button>
        </div>

        {/* <div className="background-image" /> // Bu div'in CSS'te bir karşılığı yoksa kaldırılabilir */}

        <form onSubmit={handleSubmit} className="login-box">
          <input
            placeholder="Kullanıcı adı"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
          <input
            type="password"
            placeholder="Şifre"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <button type="submit">
            {isRegistering ? "Kayıt Ol" : "Giriş Yap"}
          </button>
          {error && <p className="error">{error}</p>}
        </form>
      </div>
    </div>
  );
}

export default LoginPage;