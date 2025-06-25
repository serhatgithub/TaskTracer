import { useState } from "react";
import axios from "axios";
import "./LoginPage.css";

function LoginPage({ onLogin }) {
  const [isRegistering, setIsRegistering] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    const url = isRegistering
      ? "http://localhost:5242/api/v1/auth/register"
      : "http://localhost:5242/api/v1/auth/login";

    try {
      const res = await axios.post(url, { username, password });

      if (isRegistering) {
        alert("Kayıt başarılı! Şimdi giriş yapabilirsin.");
        setIsRegistering(false);
        return;
      }

      localStorage.setItem("token", res.data.token);
      localStorage.setItem("username", username);
      onLogin();
    } catch (err) {
      setError(
        isRegistering
          ? "Bu kullanıcı adı zaten kayıtlı."
          : "Kullanıcı adı veya şifre hatalı."
      );
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
            onClick={() => setIsRegistering(!isRegistering)}
          >
            {isRegistering ? "Giriş" : "Kayıt"}
          </button>
        </div>

        <div className="background-image" />

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
