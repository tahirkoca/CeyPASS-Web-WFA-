import React from "react";
import { PageHeader } from "../PageHeader";
import { useHeaderQuickMenu } from "../HeaderQuickMenu";
import { useNotificationsContext } from "../NotificationsProvider";
import { useShellActions } from "../ShellActions";

export function PersonelHeader(props: {
  title: string;
  subtitle?: string;
  onOpenMenu?: () => void;
  onOpenTips?: () => void;
}) {
  const quickMenu = useHeaderQuickMenu();
  const notif = useNotificationsContext();
  const shell = useShellActions();
  const onTips = props.onOpenTips ?? shell?.openTips;
  return (
    <>
      <PageHeader
        title={props.title}
        subtitle={props.subtitle}
        onOpenMenu={props.onOpenMenu}
        rightIcon="bell-outline"
        rightBadge={notif.unreadCount}
        onRightPress={() => quickMenu.open("notif")}
        rightIcon2={onTips ? "help-circle-outline" : undefined}
        onRightPress2={onTips}
      />
      {quickMenu.modal}
    </>
  );
}

