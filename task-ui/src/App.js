import { useState, useEffect } from "react";
import LoginPage from "./LoginPage";
import TasksPage from "./TasksPage";
import UserPage from "./UserPage";

function App() {
  const [screen, setScreen] = useState("loading");

  useEffect(() => {
    const token = localStorage.getItem("token");

    if (!token) {
      setScreen("login");
    } else {
      setScreen("tasks");
    }
  }, []);

  const handleLogout = () => {
    localStorage.clear();
    setScreen("login");
  };

  if (screen === "loading") return <p>Yükleniyor...</p>;

  if (screen === "login")
    return <LoginPage onLogin={() => setScreen("tasks")} />;

  if (screen === "user")
    return <UserPage onBack={() => setScreen("tasks")} />;
  if (screen === "tasks")
    return <TasksPage onUserClick={() => setScreen("user")} />;
  
  return <TasksPage onGoUser={() => setScreen("user")} onLogout={handleLogout} />;
}

export default App;
