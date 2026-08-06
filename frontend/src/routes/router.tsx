import { createBrowserRouter } from "react-router-dom";
import { LandingPage } from "../pages/LandingPage";
import { SignInPage } from "../pages/SignInPage";
import { SignUpPage } from "../pages/SignUpPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { DashboardPage } from "../pages/DashboardPage";
import { DestinationGuideSearchPage } from "../pages/DestinationGuideSearchPage";
import { DestinationGuideResultPage } from "../pages/DestinationGuideResultPage";
import { DiscoveryFeedPage } from "../pages/DiscoveryFeedPage";
import { ProfilePage } from "../pages/ProfilePage";
import { AuthGuard } from "../components/AuthGuard";
import { DashboardShell } from "../components/DashboardShell";

export const router = createBrowserRouter([
  { path: "/", element: <LandingPage /> },
  { path: "/signin", element: <SignInPage /> },
  { path: "/signup", element: <SignUpPage /> },
  {
    element: <AuthGuard />,
    children: [
      {
        element: <DashboardShell />,
        children: [
          { path: "/dashboard", element: <DashboardPage /> },
          { path: "/guide", element: <DestinationGuideSearchPage /> },
          { path: "/guide/:searchId", element: <DestinationGuideResultPage /> },
          { path: "/discovery", element: <DiscoveryFeedPage /> },
          { path: "/profile", element: <ProfilePage /> },
        ],
      },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
