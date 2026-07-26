package com.mastermind.coaching;

import android.graphics.Color;
import android.os.Bundle;

import androidx.core.view.WindowCompat;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.core.view.WindowInsetsControllerCompat;

import com.getcapacitor.BridgeActivity;

public class MainActivity extends BridgeActivity {
  @Override
  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);

    WindowCompat.setDecorFitsSystemWindows(getWindow(), false);
    getWindow().setStatusBarColor(Color.TRANSPARENT);
    getWindow().setNavigationBarColor(Color.TRANSPARENT);

    ViewCompat.setOnApplyWindowInsetsListener(getBridge().getWebView(), (view, windowInsets) -> {
      Insets systemBars = windowInsets.getInsets(WindowInsetsCompat.Type.systemBars());
      view.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
      return windowInsets;
    });
    ViewCompat.requestApplyInsets(getBridge().getWebView());

    WindowInsetsControllerCompat controller = new WindowInsetsControllerCompat(getWindow(), getWindow().getDecorView());
    controller.setAppearanceLightStatusBars(true);
    controller.setAppearanceLightNavigationBars(true);
  }
}
