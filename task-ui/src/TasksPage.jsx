// task-ui/src/TasksPage.jsx

import { useEffect, useState, useCallback } from "react";
import axios from "axios";
import "./TasksPage.css";
import { API_BASE_URL } from './apiConfig'; // Doğru şekilde import ediliyor.

function TasksPage({ onUserClick }) {
  const [tasks, setTasks] = useState([]);
  const [newTitles, setNewTitles] = useState({ 0: "", 1: "", 2: "" });
  const [filters, setFilters] = useState({ 0: "", 1: "", 2: "" });
  const [error, setError] = useState("");

  const headers = {
    Authorization: `Bearer ${localStorage.getItem("token")}`,
  };

  const loadTasks = useCallback(async () => {
    setError("");
    try {
      const res = await axios.get(`${API_BASE_URL}/tasks`, {
        headers,
      });
      setTasks(res.data);
    } catch (err) {
      console.error("Load tasks error:", err.response || err.message);
      const apiErrorMessage = err.response?.data?.message || err.response?.data || "Görevler yüklenemedi.";
      setError(apiErrorMessage);
    }
  }, []); // useCallback'in bağımlılığından headers'ı kaldırdık, çünkü her render'da değişmiyor.

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  const handleAddTask = async (status) => {
    const title = newTitles[status];
    if (!title.trim()) return;
    setError("");

    try {
      await axios.post(
        `${API_BASE_URL}/tasks`,
        { title, status },
        { headers }
      );
      setNewTitles({ ...newTitles, [status]: "" });
      loadTasks();
    } catch (err) {
      console.error("Add task error:", err.response || err.message);
      setError(err.response?.data?.message || "Görev eklenemedi.");
    }
  };

  const handleDelete = async (id) => {
    setError("");
    try {
      await axios.delete(`${API_BASE_URL}/tasks/${id}`, { headers });
      loadTasks();
    } catch (err) {
      console.error("Delete task error:", err.response || err.message);
      setError(err.response?.data?.message || "Görev silinemedi.");
    }
  };

  const changeOrder = async (id, dir) => {
    setError("");
    try {
      await axios.patch(
        `${API_BASE_URL}/tasks/${id}/order?dir=${dir}`,
        null,
        { headers }
      );
      loadTasks();
    } catch (err) {
      console.error("Change order error:", err.response || err.message);
      setError(err.response?.data?.message || "Görev sıralaması değiştirilemedi.");
    }
  };

  const changeStatus = async (id, to) => {
    setError("");
    try {
      await axios.patch(
        `${API_BASE_URL}/tasks/${id}/status?to=${to}`,
        null,
        { headers }
      );
      loadTasks();
    } catch (err) {
      console.error("Change status error:", err.response || err.message);
      setError(err.response?.data?.message || "Durum değiştirilemedi.");
    }
  };

  const grouped = {
    0: tasks.filter((t) => t.status === 0 && t.title.toLowerCase().includes(filters[0].toLowerCase())),
    1: tasks.filter((t) => t.status === 1 && t.title.toLowerCase().includes(filters[1].toLowerCase())),
    2: tasks.filter((t) => t.status === 2 && t.title.toLowerCase().includes(filters[2].toLowerCase())),
  };

  return (
    <div className="tasks-wrapper">
      <div className="tasks-container">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "30px" }}>
          <h2 className="main-title" style={{marginBottom: 0}}>Görev Yönetimi</h2>
          <button onClick={onUserClick} style={{fontSize: '1.5rem', background: 'none', border: 'none', cursor: 'pointer'}}>👤</button>
        </div>

        {error && <p className="error" style={{textAlign: 'center', marginBottom: '15px'}}>{error}</p>}

        <div className="task-columns">
          {["Todo", "Doing", "Done"].map((name, statusKey) => (
            <div
              className={`task-column ${statusKey === 1 ? "middle-column" : ""} ${statusKey === 2 ? "done-column" : ""}`}
              key={statusKey}
            >
              <h3>
                {statusKey === 0 ? "📋" : statusKey === 1 ? "🔄" : "✅"} {name}
              </h3>
              <input
                className="filter-input"
                placeholder="Görevleri filtrele..."
                value={filters[statusKey] || ""}
                onChange={(e) =>
                  setFilters({ ...filters, [statusKey]: e.target.value })
                }
              />
              <div className="task-input">
                <input
                  placeholder="Yeni görev başlığı..."
                  value={newTitles[statusKey] || ""}
                  onChange={(e) =>
                    setNewTitles({ ...newTitles, [statusKey]: e.target.value })
                  }
                  onKeyDown={(e) => e.key === "Enter" && handleAddTask(statusKey)}
                />
                <button onClick={() => handleAddTask(statusKey)}>Ekle</button>
              </div>
              <ul>
                {grouped[statusKey].map((t, index) => (
                  <li key={t.id}>
                    <span style={{ flexGrow: 1 }}>{t.title}</span>
                    <div className="actions">
                      <button title="Yukarı Taşı" onClick={() => changeOrder(t.id, "up")} disabled={index === 0}>⬆️</button>
                      <button title="Aşağı Taşı" onClick={() => changeOrder(t.id, "down")} disabled={index === grouped[statusKey].length - 1}>⬇️</button>
                      {statusKey > 0 && (<button title="Önceki Duruma Al" onClick={() => changeStatus(t.id, statusKey - 1)}>◀️</button>)}
                      {statusKey < 2 && (<button title="Sonraki Duruma Al" onClick={() => changeStatus(t.id, statusKey + 1)}>▶️</button>)}
                      <button title="Sil" onClick={() => handleDelete(t.id)}>🗑️</button>
                    </div>
                  </li>
                ))}
                {grouped[statusKey].length === 0 && <li style={{textAlign: 'center', color: '#777', fontStyle: 'italic'}}>Bu kategoride görev yok.</li>}
              </ul>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export default TasksPage;