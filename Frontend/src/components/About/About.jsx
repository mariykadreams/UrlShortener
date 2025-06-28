import React from 'react';
import './About.css';

const About = () => {
  return (
    <div className="about-container">
      <h1 className="about-title">URL Shortener Algorithm Description</h1>
      
      <p className="about-paragraph">
        The URL Shortener Service generates a unique, fixed-length short code for each original URL. 
        The algorithm ensures that each short URL is compact, unique, and easy to use.
      </p>

      <h2 className="about-subtitle">How the Algorithm Works</h2>
      <ul className="about-list">
        <li>For every long URL submitted, the application generates a unique short code.</li>
        <li>The short code is always <strong>7 characters</strong> long.</li>
        <li>It uses a <strong>Base62</strong> character set: <code>0-9, A-Z, and a-z</code>.</li>
        <li>The original URL is hashed using the <strong>SHA-256</strong> algorithm.</li>
        <li>A portion of the hash is converted into a Base62 string.</li>
      </ul>

      <h3 className="about-subsection">Length Adjustment</h3>
      <ul className="about-list">
        <li>If the code is longer than 7 characters, it is <strong>truncated</strong>.</li>
        <li>If it is shorter, it is <strong>padded with 0s</strong>.</li>
        <li>If the code already exists, the algorithm retries up to 5 times with a new input (using a GUID).</li>
      </ul>

      <h2 className="about-subtitle">From Short Code to Full Short URL</h2>
      <p className="about-paragraph">
        The generated short code is a 7-character string, for example:
        <code className="code-snippet"> 9Alk8DE</code><br />
        It becomes a full URL by combining it with a domain:
      </p>
      <ul className="about-list">
        <li><code className="code-snippet">https://your-domain.com/9Alk8DE</code></li>
        <li>During development: <code className="code-snippet">https://localhost:7283/9Alk8DE</code></li>
      </ul>

      <h2 className="about-subtitle">Clean URL Routing in ASP.NET Core</h2>
      <p className="about-paragraph">
        The app uses a clean route for short links:
        <code className="code-snippet">https://localhost:7283/9Alk8DE</code>
      </p>
      <ul className="about-list">
        <li>Extracts the short code from the path.</li>
        <li>Looks up the original URL in the database.</li>
        <li>Redirects if found, or returns <strong>404</strong> if not.</li>
      </ul>

      <p className="about-footer">
        This setup avoids longer paths like <code className="code-snippet">/api/urls/redirect/{'{code}'}</code> 
        and produces cleaner, more shareable URLs.
      </p>
    </div>
  );
};

export default About;
