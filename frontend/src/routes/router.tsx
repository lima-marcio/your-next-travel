import { createBrowserRouter } from "react-router-dom";
import { LandingPage } from "../pages/LandingPage";
import { SignInPage } from "../pages/SignInPage";
import { SignUpPage } from "../pages/SignUpPage";
import { NotFoundPage } from "../pages/NotFoundPage";

export const router = createBrowserRouter([
  { path: "/", element: <LandingPage /> },
  { path: "/signin", element: <SignInPage /> },
  { path: "/signup", element: <SignUpPage /> },
  { path: "*", element: <NotFoundPage /> },
]);
