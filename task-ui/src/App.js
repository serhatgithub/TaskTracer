import { useState, useEffect } from "react";
import LoginPage from "./LoginPage";
import TasksPage from "./TasksPage";
import UserPage from "./UserPage";

function App() {
  const [screen, setScreen] = useState("loading");

  useEffect(() => {
    const token = localStorage.getItem("token");
    setScreen(token ? "tasks" : "login");
  }, []);

  if (screen === "loading") return <p>Yükleniyor...</p>;

  if (screen === "login")
    return <LoginPage onLogin={() => setScreen("tasks")} />;

  if (screen === "user")
    return <UserPage onBack={() => setScreen("tasks")} />;

  if (screen === "tasks")
    return <TasksPage onUserClick={() => setScreen("user")} />;

  return null; // hiç ulaşılmaz, ama safety için
}

export default App;
