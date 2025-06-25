import { useEffect, useState } from "react";
import axios from "axios";
import "./TasksPage.css";

function TasksPage({ onUserClick }) {
  const [tasks, setTasks] = useState([]);
  const [newTitles, setNewTitles] = useState({ 0: "", 1: "", 2: "" });
  const [error, setError] = useState("");

  const token = localStorage.getItem("token");
  const headers = { Authorization: "Bearer " + token };

  const loadTasks = async () => {
    try {
      const res = await axios.get("http://localhost:5242/api/v1/tasks", {
        headers,
      });
      setTasks(res.data);
    } catch {
      setError("Görevler yüklenemedi.");
    }
  };

  useEffect(() => {
    loadTasks();
  }, []);

  const handleAddTask = async (status) => {
    const title = newTitles[status];
    if (!title.trim()) return;
    try {
      await axios.post(
        "http://localhost:5242/api/v1/tasks",
        { title, status },
        { headers }
      );
      setNewTitles({ ...newTitles, [status]: "" });
      loadTasks();
    } catch {
      setError("Görev eklenemedi.");
    }
  };

  const handleDelete = async (id) => {
    try {
      await axios.delete(`http://localhost:5242/api/v1/tasks/${id}`, {
        headers,
      });
      loadTasks();
    } catch {
      setError("Görev silinemedi.");
    }
  };

  const changeOrder = async (id, dir) => {
    try {
      await axios.patch(
        `http://localhost:5242/api/v1/tasks/${id}/order?dir=${dir}`,
        null,
        { headers }
      );
      loadTasks();
    } catch {
      setError("Görev sıralaması değiştirilemedi.");
    }
  };

  const changeStatus = async (id, to) => {
    try {
      await axios.patch(
        `http://localhost:5242/api/v1/tasks/${id}/status?to=${to}`,
        null,
        { headers }
      );
      loadTasks();
    } catch {
      setError("Durum değiştirilemedi.");
    }
  };

  const grouped = {
    0: tasks.filter((t) => t.status === 0),
    1: tasks.filter((t) => t.status === 1),
    2: tasks.filter((t) => t.status === 2),
  };

  return (
    <div className="tasks-wrapper">
      <div className="tasks-container">
        <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "20px" }}>
          <h2 className="main-title">Görev Yönetimi</h2>
          <button onClick={onUserClick}>👤</button>
        </div>

        {error && <p className="error">{error}</p>}

        <div className="task-columns">
          {["Todo", "Doing", "Done"].map((name, status) => (
            <div
              className={`task-column ${status === 1 ? "middle-column" : ""}`}
              key={status}
            >
              <h3>
                {status === 0 ? "📋" : status === 1 ? "🔄" : "✅"} {name}
              </h3>

              <div className="task-input">
                <input
                  placeholder="Yeni görev başlığı..."
                  value={newTitles[status] || ""}
                  onChange={(e) =>
                    setNewTitles({ ...newTitles, [status]: e.target.value })
                  }
                  onKeyDown={(e) => e.key === "Enter" && handleAddTask(status)}
                />
                <button onClick={() => handleAddTask(status)}>Ekle</button>
              </div>

              <ul>
                {grouped[status].map((t, index) => (
                  <li key={t.id}>
                    {t.title}
                    <div className="actions">
                      <button
                        onClick={() => changeOrder(t.id, "up")}
                        disabled={index === 0}
                      >
                        ⬆️
                      </button>
                      <button
                        onClick={() => changeOrder(t.id, "down")}
                        disabled={index === grouped[status].length - 1}
                      >
                        ⬇️
                      </button>
                      {status > 0 && (
                        <button onClick={() => changeStatus(t.id, status - 1)}>
                          ◀️
                        </button>
                      )}
                      {status < 2 && (
                        <button onClick={() => changeStatus(t.id, status + 1)}>
                          ▶️
                        </button>
                      )}
                      <button onClick={() => handleDelete(t.id)}>🗑️</button>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export default TasksPage;
