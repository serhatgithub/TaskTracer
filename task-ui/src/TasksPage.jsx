// task-ui/src/TasksPage.jsx
import { useEffect, useState, useCallback } from "react"; // useCallback eklendi
import axios from "axios";
import "./TasksPage.css";
import { API_BASE_URL } from './apiConfig'; // API_BASE_URL'i import ediyoruz

function TasksPage({ onUserClick }) {
  const [tasks, setTasks] = useState([]);
  const [newTitles, setNewTitles] = useState({ 0: "", 1: "", 2: "" });
  const [filters, setFilters] = useState({ 0: "", 1: "", 2: "" });
  const [error, setError] = useState("");

  // headers nesnesini her renderda yeniden oluşturmamak için useCallback içine alabiliriz
  // veya doğrudan fonksiyonlar içinde tanımlayabiliriz. Şimdilik sabit bırakıyorum.
  const headers = {
    Authorization: `Bearer ${localStorage.getItem("token")}`,
  };

  // loadTasks fonksiyonunu useCallback ile sarmalayarak useEffect bağımlılık sorunlarını önleyebiliriz.
  const loadTasks = useCallback(async () => {
    setError(""); // Her yüklemede hatayı temizle
    try {
      const res = await axios.get(`${API_BASE_URL}/tasks`, { // URL güncellendi
        headers,
      });
      setTasks(res.data);
    } catch (err) {
      console.error("Load tasks error:", err.response || err.message); // Hata ayıklama için log
      // TaskController'dan dönen hata mesajı yapısını kontrol edelim.
      // Genellikle { message: "..." } veya doğrudan string dönebilir.
      const apiErrorMessage = err.response?.data?.message || err.response?.data || "Görevler yüklenemedi.";
      setError(apiErrorMessage);
      if (err.response?.status === 401) {
        // Token geçersizse veya yoksa kullanıcıyı login sayfasına yönlendirebiliriz.
        // Bu App.js seviyesinde de yönetilebilir.
        console.warn("Unauthorized access to tasks, redirecting to login might be needed.");
      }
    }
  }, [headers]); // headers değişirse fonksiyon yeniden oluşturulur (token değişirse gibi)

  useEffect(() => {
    loadTasks();
  }, [loadTasks]); // loadTasks bağımlılık olarak eklendi

  const handleAddTask = async (status) => {
    const title = newTitles[status];
    if (!title.trim()) return;
    setError("");

    try {
      await axios.post(
        `${API_BASE_URL}/tasks`, // URL güncellendi
        { title, status },
        { headers }
      );
      setNewTitles({ ...newTitles, [status]: "" });
      loadTasks(); // Görevleri yeniden yükle
    } catch (err) {
      console.error("Add task error:", err.response || err.message);
      const apiErrorMessage = err.response?.data?.message || err.response?.data || "Görev eklenemedi.";
      setError(apiErrorMessage);
    }
  };

  const handleDelete = async (id) => {
    setError("");
    try {
      await axios.delete(`${API_BASE_URL}/tasks/${id}`, { // URL güncellendi
        headers,
      });
      loadTasks(); // Görevleri yeniden yükle
    } catch (err) {
      console.error("Delete task error:", err.response || err.message);
      const apiErrorMessage = err.response?.data?.message || err.response?.data || "Görev silinemedi.";
      setError(apiErrorMessage);
    }
  };

  const changeOrder = async (id, dir) => {
    setError("");
    try {
      await axios.patch(
        `${API_BASE_URL}/tasks/${id}/order?dir=${dir}`, // URL güncellendi
        null, // PATCH isteğinde body null olabilir
        { headers }
      );
      loadTasks(); // Görevleri yeniden yükle
    } catch (err) {
      console.error("Change order error:", err.response || err.message);
      const apiErrorMessage = err.response?.data?.message || err.response?.data || "Görev sıralaması değiştirilemedi.";
      setError(apiErrorMessage);
    }
  };

  const changeStatus = async (id, to) => {
    setError("");
    try {
      await axios.patch(
        `${API_BASE_URL}/tasks/${id}/status?to=${to}`, // URL güncellendi
        null, // PATCH isteğinde body null olabilir
        { headers }
      );
      loadTasks(); // Görevleri yeniden yükle
    } catch (err) {
      console.error("Change status error:", err.response || err.message);
      const apiErrorMessage = err.response?.data?.message || err.response?.data || "Durum değiştirilemedi.";
      setError(apiErrorMessage);
    }
  };

  // Filtrelenmiş görevler (Bu kısım aynı kalabilir)
  const grouped = {
    0: tasks.filter((t) => t.status === 0 && t.title.toLowerCase().includes(filters[0].toLowerCase())),
    1: tasks.filter((t) => t.status === 1 && t.title.toLowerCase().includes(filters[1].toLowerCase())),
    2: tasks.filter((t) => t.status === 2 && t.title.toLowerCase().includes(filters[2].toLowerCase())),
  };

  return (
    <div className="tasks-wrapper">
      <div className="tasks-container">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "30px" }}> {/* alignItems ve marginBottom güncellendi */}
          <h2 className="main-title" style={{marginBottom: 0}}>Görev Yönetimi</h2> {/* Başlık için margin bottom sıfırlandı */}
          <button onClick={onUserClick} style={{fontSize: '1.5rem', background: 'none', border: 'none', cursor: 'pointer'}}>👤</button> {/* Buton stili güncellendi */}
        </div>

        {error && <p className="error" style={{textAlign: 'center', marginBottom: '15px'}}>{error}</p>} {/* Hata mesajı stili */}

        <div className="task-columns">
          {["Todo", "Doing", "Done"].map((name, statusKey) => ( // status -> statusKey olarak değiştirildi (shadowing önlemek için)
            <div
              className={`task-column ${statusKey === 1 ? "middle-column" : ""} ${statusKey === 2 ? "done-column" : ""}`} // done-column class'ı eklendi
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
                    <span style={{ flexGrow: 1 }}>{t.title}</span> {/* Başlığın tüm alanı kaplaması için */}
                    <div className="actions">
                      <button
                        title="Yukarı Taşı"
                        onClick={() => changeOrder(t.id, "up")}
                        disabled={index === 0}
                      >
                        ⬆️
                      </button>
                      <button
                        title="Aşağı Taşı"
                        onClick={() => changeOrder(t.id, "down")}
                        disabled={index === grouped[statusKey].length - 1}
                      >
                        ⬇️
                      </button>
                      {statusKey > 0 && (
                        <button title="Önceki Duruma Al" onClick={() => changeStatus(t.id, statusKey - 1)}>
                          ◀️
                        </button>
                      )}
                      {statusKey < 2 && (
                        <button title="Sonraki Duruma Al" onClick={() => changeStatus(t.id, statusKey + 1)}>
                          ▶️
                        </button>
                      )}
                      <button title="Sil" onClick={() => handleDelete(t.id)}>🗑️</button>
                    </div>
                  </li>
                ))}
                {grouped[statusKey].length === 0 && <li style={{textAlign: 'center', color: '#777', fontStyle: 'italic'}}>Bu kategoride görev yok.</li>} {/* Boş liste mesajı */}
              </ul>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export default TasksPage;