import React from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import App from './App.jsx'
import LoginPage from './pages/LoginPage.jsx'

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      {
        index: true,
        element: <Navigate to="/login" replace />, 
      },
      {
        path: 'login',
        element: <LoginPage />,
      },
    ],
  },
])

export default router
