import './css/site.css';
import 'bootstrap-loader';
import 'url-search-params-polyfill';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Game } from './components/Game';

function renderApp() {
    const playerName = new URLSearchParams(new URL(window.location.toString()).search.slice(1)).get("Name") || '';
    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href')!;
    ReactDOM.render(
        <Game playerName={playerName} baseUrl={baseUrl} />,
        document.getElementById('react-app')
    );
}

renderApp();
