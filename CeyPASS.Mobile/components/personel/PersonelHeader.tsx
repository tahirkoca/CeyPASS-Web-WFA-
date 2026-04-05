import React from "react";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";

export function PersonelHeader(props: {
  title: string;
  subtitle?: string;
  onOpenMenu?: () => void;
}) {
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();
  return (
    <>
      <PageHeader
        title={props.title}
        subtitle={props.subtitle}
        onOpenMenu={props.onOpenMenu}
        rightIcon="bell-outline"
        rightBadge={notif.unreadCount}
        rightA11yLabel="Bildirimler ve hesap"
        onRightPress={() => quickMenu.open("notif")}
      />
      {quickMenu.modal}
    </>
  );
}

