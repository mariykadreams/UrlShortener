import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import './ShortURLInfo.css';

const BASE_API_URL = 'https://localhost:7283/api/Urls';
const SHORT_URL_BASE = 'https://localhost:7283';

function ShortURLInfo() {
  const { id } = useParams(); // <--- Получаем id из URL
  const [url, setUrl] = useState(null);
  const [message, setMessage] = useState({ type: '', text: '' });
  const [loading, setLoading] = useState(false);

  const fetchUrl = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem('jwtToken');
      const headers = { 'Content-Type': 'application/json' };
      if (token) headers['Authorization'] = `Bearer ${token}`;

      const response = await fetch(`${BASE_API_URL}/${id}`, { headers });
      if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
      const data = await response.json();
      setUrl(data);
    } catch (error) {
      console.error('Error fetching URL:', error);
      setMessage({ type: 'error', text: 'Failed to load URL info.' });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUrl();
  }, [id]);

  return (
    <div className="container">
      <h2 className="heading">Short URL Info</h2>
      {message.text && (
        <div className={`message ${message.type}`}>{message.text}</div>
      )}
      {loading ? (
        <div className="text-center">Loading URL info...</div>
      ) : url ? (
        <div className="table-wrapper">
          <table className="urls-table">
            <thead>
              <tr>
                <th>Original URL</th>
                <th>Shortened URL</th>
                <th>Created By</th>
                <th>Created At</th>
              </tr>
            </thead>
            <tbody>
              <tr>
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
                <td>{url.createdBy || 'Anonymous'}</td>
                <td>{new Date(url.createdDate).toLocaleString()}</td>
              </tr>
            </tbody>
          </table>
        </div>
      ) : (
        <div className="text-center">No URL info found.</div>
      )}
    </div>
  );
}

export default ShortURLInfo;
