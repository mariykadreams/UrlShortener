import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './ShortUrlsTable.css';

const BASE_API_URL = 'https://localhost:7283/api/Urls';
const SHORT_URL_BASE = 'https://localhost:7283';

const decodeToken = (token) => {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    const decoded = JSON.parse(jsonPayload);
    const userId = typeof decoded.sub === 'string' ? decoded.sub : null;
    const roles = Array.isArray(decoded.role)
      ? decoded.role.map(role => role.toLowerCase())
      : (decoded.role ? [decoded.role.toLowerCase()] : []);

    return { userId, roles };
  } catch (error) {
    console.error("Failed to decode JWT manually:", error);
    return null;
  }
};

function ShortUrlsTable({ isLoggedIn }) {
  const navigate = useNavigate();
  const [urls, setUrls] = useState([]);
  const [newOriginalUrl, setNewOriginalUrl] = useState('');
  const [message, setMessage] = useState({ type: '', text: '' });
  const [loading, setLoading] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [urlToDeleteId, setUrlToDeleteId] = useState(null);
  const [urlToDeleteOriginalUrl, setUrlToDeleteOriginalUrl] = useState('');

  const fetchUrls = async () => {
    setLoading(true);
    setMessage({ type: '', text: '' });
    try {
      const token = localStorage.getItem('jwtToken');
      const headers = { 'Content-Type': 'application/json' };
      if (token) headers['Authorization'] = `Bearer ${token}`;
      const response = await fetch(BASE_API_URL, { headers });
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
      const data = await response.json();
      setUrls(data);
    } catch (error) {
      console.error('Error fetching URLs:', error);
      setMessage({ type: 'error', text: 'Failed to load URLs. Please try again later.' });
    } finally {
      setLoading(false);
    }
  };

  const handleAddUrl = async (e) => {
    e.preventDefault();
    if (!newOriginalUrl.trim()) {
      setMessage({ type: 'error', text: 'Please enter a URL to shorten.' });
      return;
    }
    if (!isLoggedIn) {
      setMessage({ type: 'error', text: 'You must be logged in to add new URLs.' });
      return;
    }

    setLoading(true);
    setMessage({ type: '', text: '' });
    try {
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        setMessage({ type: 'error', text: 'Authentication token not found. Please log in.' });
        return;
      }

      const response = await fetch(`${BASE_API_URL}/add`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ originalUrl: newOriginalUrl }),
      });

      if (response.status === 409) {
        setMessage({ type: 'error', text: 'This URL has already been shortened.' });
      } else if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      } else {
        setMessage({ type: 'success', text: 'URL shortened successfully!' });
        setNewOriginalUrl('');
        await fetchUrls();
      }
    } catch (error) {
      console.error('Error adding URL:', error);
      setMessage({ type: 'error', text: `Failed to add URL: ${error.message || 'Unknown error'}` });
    } finally {
      setLoading(false);
    }
  };

  const confirmDelete = (id, originalUrl) => {
    setUrlToDeleteId(id);
    setUrlToDeleteOriginalUrl(originalUrl);
    setShowConfirmModal(true);
  };

  const handleDeleteUrl = async () => {
    if (!urlToDeleteId) return;

    setLoading(true);
    setMessage({ type: '', text: '' });
    setShowConfirmModal(false);

    try {
      const token = localStorage.getItem('jwtToken');
      if (!token) {
        setMessage({ type: 'error', text: 'Authentication token not found. Please log in.' });
        return;
      }

      const response = await fetch(`${BASE_API_URL}/${urlToDeleteId}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });

      if (!response.ok) {
        if (response.status === 403) {
          setMessage({ type: 'error', text: 'You are not authorized to delete this URL.' });
        } else if (response.status === 404) {
          setMessage({ type: 'error', text: 'URL not found.' });
        } else {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
      } else {
        setMessage({ type: 'success', text: 'URL deleted successfully!' });
        await fetchUrls();
      }
    } catch (error) {
      console.error('Error deleting URL:', error);
      setMessage({ type: 'error', text: `Failed to delete URL: ${error.message || 'Unknown error'}` });
    } finally {
      setLoading(false);
      setUrlToDeleteId(null);
      setUrlToDeleteOriginalUrl('');
    }
  };

  useEffect(() => {
    fetchUrls();
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const user = decodeToken(token);
      setCurrentUser(user);
    } else {
      setCurrentUser(null);
    }
  }, [isLoggedIn]);

  // Кнопка Delete всегда активна, не дизейблится
  const canDeleteUrl = () => true;

  return (
    <div className="container">
      <h2 className="heading">Short URLs Table</h2>

      {message.text && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}

      {isLoggedIn && (
        <div className="add-url-section">
          <h3 className="add-url-title">Add New URL</h3>
          <form onSubmit={handleAddUrl} className="add-url-form">
            <input
              type="url"
              value={newOriginalUrl}
              onChange={(e) => setNewOriginalUrl(e.target.value)}
              placeholder="Enter original URL"
              required
            />
            <button type="submit" disabled={loading}>
              {loading ? 'Adding...' : 'Shorten URL'}
            </button>
          </form>
        </div>
      )}

      {loading && urls.length === 0 ? (
        <div className="text-center">Loading URLs...</div>
      ) : urls.length === 0 ? (
        <div className="text-center">No URLs found.</div>
      ) : (
        <div className="table-wrapper">
          <table className="urls-table">
            <thead>
              <tr>
                <th>Original URL</th>
                <th>Shortened URL</th>
                {isLoggedIn && <th className="actions-cell">Actions</th>}
              </tr>
            </thead>
            <tbody>
              {urls.map((url) => (
                <tr key={url.id}>
                  <td>
                    <a href={url.originalUrl} target="_blank" rel="noopener noreferrer">
                      {url.originalUrl}
                    </a>
                  </td>
                  <td>
                    <a href={`${SHORT_URL_BASE}/${url.shortCode}`} target="_blank" rel="noopener noreferrer">
                      {`${SHORT_URL_BASE}/${url.shortCode}`}
                    </a>
                  </td>
                  {isLoggedIn && (
                    <td className="actions-cell">
                      <button
                        className="action-button view"
                        onClick={() => navigate(`/ShortUrlInfo/${url.id}`)}
                      >
                        View
                      </button>
                      <button
                        className="action-button delete"
                        onClick={() => confirmDelete(url.id, url.originalUrl)}
                        // Кнопка всегда активна, поэтому disabled нет
                      >
                        Delete
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showConfirmModal && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>Confirm Deletion</h3>
            <p>
              Are you sure you want to delete this URL?
              <br />
              <span>{urlToDeleteOriginalUrl}</span>
            </p>
            <div className="modal-buttons">
              <button className="delete" onClick={handleDeleteUrl}>Delete</button>
              <button className="cancel" onClick={() => setShowConfirmModal(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ShortUrlsTable;
